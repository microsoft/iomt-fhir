// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Logging.Telemetry
{
    public interface ITimed : IDisposable
    {
        ITelemetryLogger Log { get; }

        TimeSpan Elapsed { get; }

        ITimed Record();

        ITimed Record(Metric metric);
    }
}
