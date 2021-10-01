// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Search;
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
        private readonly FhirClient _client;
        private readonly IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation> _fhirTemplateProcessor;
        private readonly IMemoryCache _observationCache;
        private readonly ITelemetryLogger _logger;

        public R4FhirImportService(IResourceIdentityService resourceIdentityService, FhirClient fhirClient, IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation> fhirTemplateProcessor, IMemoryCache observationCache, ITelemetryLogger logger)
        {
            _fhirTemplateProcessor = EnsureArg.IsNotNull(fhirTemplateProcessor, nameof(fhirTemplateProcessor));
            _client = EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));
            _observationCache = EnsureArg.IsNotNull(observationCache, nameof(observationCache));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));

            ResourceIdentityService = EnsureArg.IsNotNull(resourceIdentityService, nameof(resourceIdentityService));
        }

        protected IResourceIdentityService ResourceIdentityService { get; }

        public override async Task ProcessAsync(ILookupTemplate<IFhirTemplate> config, IMeasurementGroup data, Func<Exception, IMeasurementGroup, Task<bool>> errorConsumer = null)
        {
            // Get required ids
            var ids = await ResourceIdentityService.ResolveResourceIdentitiesAsync(data).ConfigureAwait(false);

            var grps = _fhirTemplateProcessor.CreateObservationGroups(config, data);

            foreach (var grp in grps)
            {
                _ = await SaveObservationAsync(config, grp, ids).ConfigureAwait(false);
            }
        }

        public virtual async Task<string> SaveObservationAsync(ILookupTemplate<IFhirTemplate> config, IObservationGroup observationGroup, IDictionary<ResourceType, string> ids)
        {
            var identifier = GenerateObservationIdentifier(observationGroup, ids);
            var cacheKey = $"{identifier.System}|{identifier.Value}";

            if (!_observationCache.TryGetValue(cacheKey, out Model.Observation existingObservation))
            {
                existingObservation = await GetObservationFromServerAsync(identifier).ConfigureAwait(false);
            }

            Model.Observation result;
            if (existingObservation == null)
            {
                var newObservation = GenerateObservation(config, observationGroup, identifier, ids);
                result = await _client.CreateAsync(newObservation).ConfigureAwait(false);
                LogFhirResourceSavedMetrics(ResourceOperation.Created, observationGroup);
            }
            else
            {
                var policyResult = await Policy<Model.Observation>
                     .Handle<FhirOperationException>(ex => ex.Status == System.Net.HttpStatusCode.Conflict || ex.Status == System.Net.HttpStatusCode.PreconditionFailed)
                     .RetryAsync(2, async (polyRes, attempt) =>
                     {
                         existingObservation = await GetObservationFromServerAsync(identifier).ConfigureAwait(false);
                     })
                     .ExecuteAndCaptureAsync(async () =>
                     {
                         var mergedObservation = MergeObservation(config, existingObservation, observationGroup);
                         return await _client.UpdateAsync(mergedObservation, versionAware: true).ConfigureAwait(false);
                     }).ConfigureAwait(false);

                var exception = policyResult.FinalException;

                if (exception != null)
                {
                    throw exception;
                }

                result = policyResult.Result;
                LogFhirResourceSavedMetrics(ResourceOperation.Updated, observationGroup);
            }

            _observationCache.CreateEntry(cacheKey)
                   .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddHours(1))
                   .SetSize(1)
                   .SetValue(result)
                   .Dispose();

            return result.Id;
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
            var searchParams = identifier.ToSearchParams();
            var result = await _client.SearchAsync<Model.Observation>(searchParams).ConfigureAwait(false);
            return await result.ReadOneFromBundleWithContinuationAsync<Model.Observation>(_client);
        }

        private void LogFhirResourceSavedMetrics(ResourceOperation operation, IObservationGroup observationGroup, ResourceType resourceType = ResourceType.Observation)
        {
            _logger.LogMetric(IomtMetrics.FhirResourceSaved(resourceType, operation), 1);
            foreach (var measurement in observationGroup.Measurements)
            {
                var iotProcessingLatency = DateTime.UtcNow - measurement.IngestionTimeUtc.Value;
                _logger.LogMetric(IomtMetrics.IotConnectorFhirProcessingLatency(), iotProcessingLatency.TotalSeconds);
                _logger.LogMetric(IomtMetrics.IotConnectorFhirProcessingLatencyMs(), iotProcessingLatency.TotalMilliseconds);
            }
        }
    }
}