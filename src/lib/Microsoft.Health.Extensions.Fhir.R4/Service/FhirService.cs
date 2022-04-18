// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Model;
using IFhirClient = Microsoft.Health.Fhir.Client.IFhirClient;

namespace Microsoft.Health.Extensions.Fhir.Service
{
    public class FhirService : IFhirService
    {
        private readonly IFhirClient _fhirClient;

        public FhirService(IFhirClient fhirClient)
        {
            _fhirClient = EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));
        }

        public async Task<T> CreateResourceAsync<T>(
            T resource,
            string conditionalCreateCriteria = null,
            string provenanceHeader = null,
            CancellationToken cancellationToken = default)
            where T : Resource
        {
            EnsureArg.IsNotNull(resource, nameof(resource));

            return await _fhirClient.CreateAsync(resource, conditionalCreateCriteria, provenanceHeader, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Bundle> SearchForResourceAsync(
            ResourceType resourceType,
            string query = null,
            int? count = null,
            CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull<ResourceType>(resourceType, nameof(resourceType));

            return await _fhirClient.SearchAsync(resourceType, query, count, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> ReadResourceAsync<T>(
            ResourceType resourceType,
            string resourceId,
            CancellationToken cancellationToken = default)
            where T : Resource
        {
            EnsureArg.IsNotNull<ResourceType>(resourceType, nameof(resourceType));
            EnsureArg.IsNotNullOrWhiteSpace(resourceId, nameof(resourceId));

            return await _fhirClient.ReadAsync<T>(resourceType, resourceId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> ReadResourceAsync<T>(
           string uri,
           CancellationToken cancellationToken = default)
            where T : Resource
        {
            EnsureArg.IsNotNullOrWhiteSpace(uri, nameof(uri));

            return await _fhirClient.ReadAsync<T>(uri, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> UpdateResourceAsync<T>(
           T resource,
           string ifMatchVersion = null,
           string provenanceHeader = null,
           CancellationToken cancellationToken = default)
            where T : Resource
        {
            EnsureArg.IsNotNull(resource, nameof(resource));

            if (string.IsNullOrWhiteSpace(ifMatchVersion) && resource.HasVersionId)
            {
                // Underlying FhirClient already adds the W/"" formating and inserts content of the ifMatchVersion
                ifMatchVersion = resource.VersionId.ToString();
            }

            return await _fhirClient.UpdateAsync(resource, ifMatchVersion, provenanceHeader, cancellationToken).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<Bundle> IterateOverAdditionalBundlesAsync(
            Bundle bundle,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Bundle nextBundle = bundle;
            while (nextBundle?.NextLink != null)
            {
                nextBundle = await _fhirClient.SearchAsync(bundle.NextLink.ToString(), cancellationToken).ConfigureAwait(false);
                if (nextBundle != null)
                {
                    yield return nextBundle;
                }
            }
        }
    }
}
