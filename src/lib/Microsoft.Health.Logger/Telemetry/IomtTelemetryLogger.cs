// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.ApplicationInsights;

namespace Microsoft.Health.Logging.Telemetry
{
    public class IomtTelemetryLogger : ITelemetryLogger
    {
        private readonly TelemetryClient _telemetryClient;

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
                LogExceptionWithProperties(ex);
                LogInnerException(ex);
            }
        }

        public virtual void LogTrace(string message)
        {
            _telemetryClient.TrackTrace(message);
        }

        public void LogMetricWithDimensions(Common.Telemetry.Metric metric, double metricValue)
        {
            EnsureArg.IsNotNull(metric);
            _telemetryClient.LogMetric(metric, metricValue);
        }

        private void LogExceptionWithProperties(Exception ex)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            _telemetryClient.LogException(ex);
        }

        private void LogAggregateException(AggregateException e)
        {
            LogInnerException(e);

            foreach (var exception in e.InnerExceptions)
            {
                LogExceptionWithProperties(exception);
            }
        }

        private void LogInnerException(Exception ex)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));

            var innerException = ex.InnerException;
            if (innerException != null)
            {
                LogExceptionWithProperties(innerException);
            }
        }
    }
}
