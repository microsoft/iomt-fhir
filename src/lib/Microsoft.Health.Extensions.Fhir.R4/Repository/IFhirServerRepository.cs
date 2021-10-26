// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Microsoft.Health.Extensions.Fhir.Repository
{
    public interface IFhirServerRepository
    {
        Task<Bundle> SearchForResourceAsync<T>(SearchParams searchParams, CancellationToken cancellationToken = default)
            where T : Resource;

        Task<T> CreateResourceAsync<T>(T resource, CancellationToken cancellationToken = default)
            where T : Resource;

        Task<T> UpdateResourceAsync<T>(T resource, bool versionAware = false, CancellationToken cancellationToken = default)
            where T : Resource;

        /// <summary>
        /// Produces an iterator over additional Bundles associated with the passed Bundle. The original Bundle is not included in the returned iterator. The
        /// iterator completes when the first null Bundle in the chain is returned from the FhirServer
        /// </summary>
        /// <param name="bundle">The Bundle to begin iterating over</param>
        /// <param name="pageDirection">The direction to search for pages in</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>An iterator of the collection of Bundles associatd with the supplied Bundle</returns>
        IAsyncEnumerable<Bundle> IterateOverAdditionalBundlesAsync(Bundle bundle, PageDirection pageDirection = PageDirection.Next, CancellationToken cancellationToken = default);
    }
}
