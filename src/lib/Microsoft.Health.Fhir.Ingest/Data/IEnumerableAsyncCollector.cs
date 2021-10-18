// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Microsoft.Health.Fhir.Ingest
{
    public interface IEnumerableAsyncCollector<T> : IAsyncCollector<T>
    {
        /// <summary>
        /// Adds one or more items to the <see cref="T:Microsoft.Azure.WebJobs.IAsyncCollector`1" />.
        /// </summary>
        /// <param name="items">The items to be added.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that will add the item to the collector.</returns>
        Task AddAsync(IEnumerable<T> items, CancellationToken cancellationToken = default(CancellationToken));
    }
}