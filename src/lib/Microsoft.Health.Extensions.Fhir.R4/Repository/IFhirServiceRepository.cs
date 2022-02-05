// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Extensions.Fhir.Repository
{
    public interface IFhirServiceRepository
    {
        Task<T> CreateResourceAsync<T>(T resource, string conditionalCreateCriteria = null, string provenanceHeader = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : Resource;

        Task<Bundle> SearchForResourceAsync(ResourceType resourceType, string query = null, int? count = null, CancellationToken cancellationToken = default(CancellationToken));

        Task<Bundle> SearchForResourceAsync(string url, CancellationToken cancellationToken = default);

        Task<T> ReadResourceAsync<T>(ResourceType resourceType, string resourceId, CancellationToken cancellationToken = default(CancellationToken))
            where T : Resource;

        Task<T> ReadResourceAsync<T>(string uri, CancellationToken cancellationToken = default(CancellationToken))
            where T : Resource;

        Task<T> UpdateResourceAsync<T>(T resource, string ifMatchVersion = null, string provenanceHeader = null, CancellationToken cancellationToken = default)
            where T : Resource;

        /// <summary>
        /// Produces an iterator over additional Bundles associated with the passed Bundle. The original Bundle is not included in the returned iterator. The
        /// iterator completes when the first null Bundle in the chain is returned from the FhirServer
        /// </summary>
        /// <param name="bundle">The Bundle to begin iterating over</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>An iterator of the collection of Bundles associatd with the supplied Bundle</returns>
        IAsyncEnumerable<Bundle> IterateOverAdditionalBundlesAsync(Bundle bundle, CancellationToken cancellationToken = default(CancellationToken));
    }
}