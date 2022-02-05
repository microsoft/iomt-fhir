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

namespace Microsoft.Health.Extensions.Fhir.Repository
{
    public class FhirServiceRepository : IFhirServiceRepository
    {
        private readonly IFhirClient _fhirClient;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public FhirServiceRepository(IFhirClient fhirClient)
        {
            _fhirClient = EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<T> CreateResourceAsync<T>(
            T resource,
            string conditionalCreateCriteria = null,
            string provenanceHeader = null,
            CancellationToken cancellationToken = default)
            where T : Resource
        {
            EnsureArg.IsNotNull(resource, nameof(resource));
            _cancellationTokenSource.CancelAfter(60000);

            CancellationToken token = cancellationToken.Equals(null) ? _cancellationTokenSource.Token : cancellationToken;

            return await _fhirClient.CreateAsync(resource, conditionalCreateCriteria, provenanceHeader, token).ConfigureAwait(false);
        }

        public async Task<Bundle> SearchForResourceAsync(
            ResourceType resourceType,
            string query = null,
            int? count = null,
            CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull<ResourceType>(resourceType, nameof(resourceType));
            _cancellationTokenSource.CancelAfter(60000);

            CancellationToken token = cancellationToken.Equals(null) ? _cancellationTokenSource.Token : cancellationToken;

            return await _fhirClient.SearchAsync(resourceType, query, count, token).ConfigureAwait(false);
        }

        public async Task<Bundle> SearchForResourceAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrWhiteSpace(url, nameof(url));
            _cancellationTokenSource.CancelAfter(60000);

            CancellationToken token = cancellationToken.Equals(null) ? _cancellationTokenSource.Token : cancellationToken;

            return await _fhirClient.SearchAsync(url, token).ConfigureAwait(false);
        }

        public async Task<T> ReadResourceAsync<T>(
            ResourceType resourceType,
            string resourceId,
            CancellationToken cancellationToken = default)
            where T : Resource
        {
            EnsureArg.IsNotNull<ResourceType>(resourceType, nameof(resourceType));
            EnsureArg.IsNotNullOrWhiteSpace(resourceId, nameof(resourceId));
            _cancellationTokenSource.CancelAfter(60000);

            CancellationToken token = cancellationToken.Equals(null) ? _cancellationTokenSource.Token : cancellationToken;

            return await _fhirClient.ReadAsync<T>(resourceType, resourceId, token).ConfigureAwait(false);
        }

        public async Task<T> ReadResourceAsync<T>(
           string uri,
           CancellationToken cancellationToken = default)
            where T : Resource
        {
            EnsureArg.IsNotNullOrWhiteSpace(uri, nameof(uri));
            _cancellationTokenSource.CancelAfter(60000);

            CancellationToken token = cancellationToken.Equals(null) ? _cancellationTokenSource.Token : cancellationToken;

            return await _fhirClient.ReadAsync<T>(uri, token).ConfigureAwait(false);
        }

        public async Task<T> UpdateResourceAsync<T>(
           T resource,
           string ifMatchVersion = null,
           string provenanceHeader = null,
           CancellationToken cancellationToken = default)
            where T : Resource
        {
            EnsureArg.IsNotNull(resource, nameof(resource));
            _cancellationTokenSource.CancelAfter(60000);

            CancellationToken token = cancellationToken.Equals(null) ? _cancellationTokenSource.Token : cancellationToken;

            return await _fhirClient.UpdateAsync(resource, ifMatchVersion, provenanceHeader, token).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<Bundle> IterateOverAdditionalBundlesAsync(
            Bundle bundle,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (bundle.Link.Count != 0)
            {
                foreach (var link in bundle.Link)
                {
                    Bundle trackedBundle = await _fhirClient.ReadAsync<Bundle>(link.Url, cancellationToken).ConfigureAwait(false);
                    if (trackedBundle != null)
                    {
                        yield return trackedBundle;
                    }
                }
            }
        }
    }
}
