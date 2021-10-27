// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.ApplicationInsights;
using Microsoft.Health.Logging.Metrics.Telemetry;

namespace Microsoft.Health.Logging.Telemetry
{
    public class IomtTelemetryLogger : ITelemetryLogger
    {
        private TelemetryClient _telemetryClient;

        public IomtTelemetryLogger(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
            EnsureArg.IsNotNull(_telemetryClient);
        }

        public virtual void LogMetric(Common.Telemetry.Metric metric, double metricValue)
        {
            EnsureArg.IsNotNull(metric);
            LogMetricWithDimensions(metric, metricValue);
        }

        public virtual void LogError(Exception ex)
        {
            if (ex is AggregateException e)
            {
                // Address bug https://github.com/microsoft/iomt-fhir/pull/120
                LogAggregateException(e);
            }
            else
            {
                _telemetryClient.TrackException(ex);
                if (ex.InnerException != null)
                {
                    _telemetryClient.TrackException(ex.InnerException);
                }
            }
        }

        public virtual void LogTrace(string message)
        {
            _telemetryClient.TrackTrace(message);
        }

        public void LogMetricWithDimensions(Common.Telemetry.Metric metric, double metricValue)
        {
            EnsureArg.IsNotNull(metric);
            metric.LogMetric(_telemetryClient, metricValue);
        }

        private void LogAggregateException(AggregateException e)
        {
            if (e.InnerException != null)
            {
                _telemetryClient.TrackException(e.InnerException);
            }

            foreach (var exception in e.InnerExceptions)
            {
                _telemetryClient.TrackException(exception);
            }
        }
    }
}
