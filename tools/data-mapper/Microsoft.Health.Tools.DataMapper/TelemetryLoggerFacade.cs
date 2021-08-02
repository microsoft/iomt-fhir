// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Tools.DataMapper
{
    /// <summary>
    /// A facade which routes ITelemetryLogger operations to the underlying ILogger
    /// </summary>
    public class TelemetryLoggerFacade : ITelemetryLogger
    {
        private readonly ILogger<TelemetryLoggerFacade> _logger;

        public TelemetryLoggerFacade(ILogger<TelemetryLoggerFacade> logger)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
        }

        public void LogError(Exception exception)
        {
            EnsureArg.IsNotNull(exception, nameof(exception));
            _logger.LogError(exception, exception.Message);
        }

        public void LogMetric(Metric metric, double metricValue)
        {
            EnsureArg.IsNotNull(metric, nameof(metric));
            _logger.LogMetric(metric.Name, metricValue);
        }

        public void LogTrace(string message)
        {
            _logger.LogTrace(message);
        }
    }
}
