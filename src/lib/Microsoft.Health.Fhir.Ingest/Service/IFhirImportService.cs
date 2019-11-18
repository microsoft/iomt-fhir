// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public interface IFhirImportService<TData, TConfig>
    {
        Task ProcessAsync(TConfig config, TData data, Func<Exception, TData, Task<bool>> errorConsumer = null);
    }
}
