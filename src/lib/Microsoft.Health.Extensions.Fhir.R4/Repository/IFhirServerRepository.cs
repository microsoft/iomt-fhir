// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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

        Task<T> ContinueBundleAsync<T>(Bundle bundle, PageDirection pageDirection = PageDirection.Next, CancellationToken cancellationToken = default)
            where T : Resource;
    }
}
