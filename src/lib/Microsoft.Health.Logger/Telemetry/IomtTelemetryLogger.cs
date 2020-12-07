// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.ApplicationInsights;
using Microsoft.Health.Fhir.Ingest.Telemetry.Metrics;

namespace Microsoft.Health.Logger.Telemetry
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
            _telemetryClient.TrackException(ex);
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
    }
}
