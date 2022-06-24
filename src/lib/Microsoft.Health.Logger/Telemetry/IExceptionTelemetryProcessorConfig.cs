// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Logging.Telemetry
{
    public interface IExceptionTelemetryProcessorConfig
    {
        Type[] HandledExceptionTypes { get; set; }
    }
}
