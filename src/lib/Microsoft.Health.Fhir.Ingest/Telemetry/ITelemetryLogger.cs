// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public interface ITelemetryLogger
    {
        void LogMetric(string metricName, double metricValue, Dictionary<string, object> dimensions);

        void LogError(Exception ex);

        void LogTrace(string message);
    }
}
