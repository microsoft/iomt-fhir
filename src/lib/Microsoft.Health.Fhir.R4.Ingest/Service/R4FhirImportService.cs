// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Search;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions;
using Microsoft.Health.Fhir.Client;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Polly;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4FhirImportService :
        FhirImportService
    {
        private readonly IFhirService _fhirService;
        private readonly IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation> _fhirTemplateProcessor;
        private readonly IMemoryCache _observationCache;
        private readonly ITelemetryLogger _logger;

        public R4FhirImportService(
            IResourceIdentityService resourceIdentityService,
            IFhirService fhirService,
            IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation> fhirTemplateProcessor,
            IMemoryCache observationCache,
            ITelemetryLogger logger)
        {
            _fhirTemplateProcessor = EnsureArg.IsNotNull(fhirTemplateProcessor, nameof(fhirTemplateProcessor));
            _fhirService = EnsureArg.IsNotNull(fhirService, nameof(fhirService));
            _observationCache = EnsureArg.IsNotNull(observationCache, nameof(observationCache));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));

            ResourceIdentityService = EnsureArg.IsNotNull(resourceIdentityService, nameof(resourceIdentityService));
        }

        protected IResourceIdentityService ResourceIdentityService { get; }

        public override async Task ProcessAsync(ILookupTemplate<IFhirTemplate> config, IMeasurementGroup data, CancellationToken ct, Func<Exception, IMeasurementGroup, Task<bool>> errorConsumer = null)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                // Get required ids
                var ids = await ResourceIdentityService.ResolveResourceIdentitiesAsync(data).ConfigureAwait(false);

                IEnumerable<IObservationGroup> grps = null;
                try
                {
                    grps = _fhirTemplateProcessor.CreateObservationGroups(config, data);
                }
                catch (Exception ex)
                {
                    // user story 93303 to include appropriate message context for this error.
                    throw new FhirDataMappingException(ex.Message, ex, nameof(FhirDataMappingException));
                }

                foreach (var grp in grps)
                {
                    _ = await SaveObservationAsync(config, grp, ids, ct).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                FhirServiceExceptionProcessor.ProcessException(ex, _logger);
                throw;
            }
        }

        public virtual async Task<string> SaveObservationAsync(ILookupTemplate<IFhirTemplate> config, IObservationGroup observationGroup, IDictionary<ResourceType, string> ids, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var identifier = GenerateObservationIdentifier(observationGroup, ids);
            var cacheKey = $"{identifier.System}|{identifier.Value}";

            if (!_observationCache.TryGetValue(cacheKey, out Model.Observation existingObservation))
            {
                existingObservation = await GetObservationFromServerAsync(identifier).ConfigureAwait(false);
            }

            var policyResult = await Policy<(Model.Observation observation, ResourceOperation operationType)>
                .Handle<FhirException>(ex => ex.StatusCode == System.Net.HttpStatusCode.Conflict || ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                .RetryAsync(2, async (polyRes, attempt) =>
                {
                    // 409 Conflict or 412 Precondition Failed can occur if the Observation.meta.versionId does not match the update request.
                    // This can happen if 2 independent processes are updating the same Observation simultaneously.
                    // or
                    // The update operation failed because the Observation no longer exists.
                    // This can happen if a cached Observation was deleted from the FHIR Server.

                    _logger.LogTrace("A conflict or precondition caused an Observation update to fail. Getting the most recent Observation.");

                    // Attempt to get the most recent version of the Observation.
                    existingObservation = await GetObservationFromServerAsync(identifier).ConfigureAwait(false);

                    // If the Observation no longer exists on the FHIR Server, it was most likely deleted.
                    if (existingObservation == null)
                    {
                        _logger.LogTrace("A cached version of an Observation was deleted. Creating a new Observation.");

                        // Remove the Observation from the cache (this version no longer exists on the FHIR Server.
                        _observationCache.Remove(cacheKey);
                    }
                })
                .ExecuteAndCaptureAsync(
                    async (cancellationToken) =>
                    {
                        if (existingObservation == null)
                        {
                            var newObservation = GenerateObservation(config, observationGroup, identifier, ids);

                            ct.ThrowIfCancellationRequested();

                            var result = await _fhirService.UpdateResourceAsync(newObservation).ConfigureAwait(false);

                            // If the version id is not equal to "1" on a create then it is likely two processes modified
                            // the resource at the same time.
                            // However we cannot check for not equal to "1" because a resource can be soft-deleted and recreated
                            // and on the recreate the resource version will also be not equal to "1", so instead we will check for resource version id = "2"
                            if (result.VersionId == "2")
                            {
                                _logger.LogMetric(IomtMetrics.FHIRResourceContention(ResourceType.Observation), 1);
                            }

                            return (result, ResourceOperation.Created);
                        }

                        // Merge the new data with the existing Observation.
                        var mergedObservation = MergeObservation(config, existingObservation, observationGroup);

                        // Check to see if there are any changes after merging and update the Status to amended if changed.
                        if (!existingObservation.AmendIfChanged(mergedObservation))
                        {
                            // There are no changes to the Observation - Do not update.
                            return (existingObservation, ResourceOperation.NoOperation);
                        }

                        // Update the Observation. Some failures will be handled in the RetryAsync block above.
                        return (await _fhirService.UpdateResourceAsync(mergedObservation).ConfigureAwait(false), ResourceOperation.Updated);
                    }, cancellationToken: ct).ConfigureAwait(false);

            var exception = policyResult.FinalException;

            if (exception != null)
            {
                throw exception;
            }

            var observation = policyResult.Result.observation;

            _logger.LogMetric(IomtMetrics.FhirResourceSaved(ResourceType.Observation, policyResult.Result.operationType), 1);

            _observationCache.CreateEntry(cacheKey)
                   .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddHours(1))
                   .SetSize(1)
                   .SetValue(observation)
                   .Dispose();

            return observation.Id;
        }

        public virtual Model.Observation GenerateObservation(ILookupTemplate<IFhirTemplate> config, IObservationGroup grp, Model.Identifier observationId, IDictionary<ResourceType, string> ids)
        {
            EnsureArg.IsNotNull(grp, nameof(grp));
            EnsureArg.IsNotNull(observationId, nameof(observationId));
            EnsureArg.IsNotNull(ids, nameof(ids));

            var patientId = Ensure.String.IsNotNullOrWhiteSpace(ids[ResourceType.Patient], nameof(ResourceType.Patient));
            var deviceId = Ensure.String.IsNotNullOrWhiteSpace(ids[ResourceType.Device], nameof(ResourceType.Device));

            var observation = _fhirTemplateProcessor.CreateObservation(config, grp);
            observation.Subject = patientId.ToReference<Model.Patient>();
            observation.Device = deviceId.ToReference<Model.Device>();
            observation.Identifier = new List<Model.Identifier> { observationId };
            observation.Id = SHA256HashGenerator.ComputeHashForIdentifier(observationId);

            if (ids.TryGetValue(ResourceType.Encounter, out string encounterId))
            {
                observation.Encounter = encounterId.ToReference<Model.Encounter>();
            }

            return observation;
        }

        public virtual Model.Observation MergeObservation(ILookupTemplate<IFhirTemplate> config, Model.Observation observation, IObservationGroup grp)
        {
            return _fhirTemplateProcessor.MergeObservation(config, grp, observation);
        }

        protected static Model.Identifier GenerateObservationIdentifier(IObservationGroup grp, IDictionary<ResourceType, string> ids)
        {
            EnsureArg.IsNotNull(grp, nameof(grp));
            EnsureArg.IsNotNull(ids, nameof(ids));

            var identity = GenerateObservationId(grp, ids[ResourceType.Device], ids[ResourceType.Patient]);
            return new Model.Identifier
            {
                System = identity.System,
                Value = identity.Identifer,
            };
        }

        protected virtual async Task<Model.Observation> GetObservationFromServerAsync(Model.Identifier identifier)
        {
            var result = await _fhirService.SearchForResourceAsync(Model.ResourceType.Observation, identifier.ToSearchQueryParameter()).ConfigureAwait(false);

            var foundObservations = (await result.ReadFromBundleWithContinuationAsync<Model.Observation>(_fhirService).ConfigureAwait(false))
                .ToArray();

            if (foundObservations.Length == 0)
            {
                return null;
            }

            // Discovered an issue where FHIR Service is only matching on first 128 characters of the identifier.
            // Match observations that match the desired identifier.
            // This will also prevent merging observations that share the first 128 characters.

            var matchedObservations = foundObservations
                .Where(obs => obs.Identifier.Any(id => identifier.IsExactly(id)))
                .ToArray();

            if (matchedObservations.Length == 0)
            {
                return null;
            }

            if (matchedObservations.Length == 1)
            {
                return matchedObservations[0];
            }

            // More than one Observation found with the same identifier.
            throw new MultipleResourceFoundException<Model.Observation>(matchedObservations.Length, matchedObservations.Select(obs => obs.ToMetadata()));
        }
    }
}
