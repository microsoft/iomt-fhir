// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Tools.EventDebugger
{
    public class SimpleTelemetryLogger : ITelemetryLogger
    {
        private ILogger<SimpleTelemetryLogger> _logger;

        public SimpleTelemetryLogger(ILogger<SimpleTelemetryLogger> logger)
        {
            _logger = logger;
        }

        public void LogMetric(Metric metric, double metricValue)
        {
        }

        public void LogError(Exception ex)
        {
            _logger.LogError(ex, "Forwarding exception");
        }

        public void LogTrace(string message)
        {
            _logger.LogInformation(message);
        }
    }
}