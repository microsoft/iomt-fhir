// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public interface IDataNormalizationService<TData, TOutput>
    {
        Task ProcessAsync(IEnumerable<TData> data, IAsyncCollector<TOutput> collector, Func<Exception, TData, Task<bool>> errorConsumer = null);

        Task ProcessAsync(IEnumerable<TData> data, IEnumerableAsyncCollector<TOutput> collector, Func<Exception, TData, Task<bool>> errorConsumer = null);
    }
}
