// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.WebJobs;

namespace Microsoft.Health.Fhir.Ingest
{
    public class EnumerableAsyncCollectorFacade<T> : IEnumerableAsyncCollector<T>
    {
        private readonly IAsyncCollector<T> _wrappedAsyncCollector;

        public EnumerableAsyncCollectorFacade(IAsyncCollector<T> asyncCollector)
        {
            _wrappedAsyncCollector = EnsureArg.IsNotNull(asyncCollector, nameof(asyncCollector));
        }

        public async Task AddAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(items, nameof(items));
            var tasks = items.Select(async i => await AddAsync(i, cancellationToken));
            await Task.WhenAll(tasks);
        }

        public async Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            await _wrappedAsyncCollector.AddAsync(item, cancellationToken);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}