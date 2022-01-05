// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public interface IConversionResultWriter
    {
        Task StoreConversionResult(DebugValidationResult conversionResult, CancellationToken cancellationToken = default);
    }
}