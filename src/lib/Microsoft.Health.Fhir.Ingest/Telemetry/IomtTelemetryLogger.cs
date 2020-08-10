// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class IomtTelemetryLogger : ITelemetryLogger
    {
        private TelemetryClient _telemetryClient;
        private readonly string _namespace = "IoMT";

        public IomtTelemetryLogger(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
            EnsureArg.IsNotNull(_telemetryClient);
        }

        public virtual void LogMetric(Metrics.Metric metric, double metricValue)
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

        public void LogMetricWithDimensions(Metrics.Metric metric, double metricValue)
        {
            EnsureArg.IsNotNull(metric);

            var metricName = metric.Name;
            var dimensions = metric.Dimensions;
            var dimensionNumber = metric.Dimensions.Count;

            if (dimensionNumber > 10)
            {
                _telemetryClient.TrackException(
                    new Exception($"Metric {metricName} exceeds the amount of allowed dimensions"));
                return;
            }

            string[] dimNames = new string[dimensions.Count];
            dimensions.Keys.CopyTo(dimNames, 0);

            string[] dimValues = new string[dimensions.Count];
            dimensions.Values.CopyTo(dimValues, 0);

            switch (dimensionNumber)
            {
                case 0:
                    _telemetryClient
                        .GetMetric(
                            metricName)
                        .TrackValue(
                            metricValue);
                    break;
                case 1:
                    _telemetryClient
                        .GetMetric(
                            metricName,
                            dimNames[0])
                        .TrackValue(
                            metricValue,
                            dimValues[0]);
                    break;
                case 2:
                    _telemetryClient
                        .GetMetric(
                            metricName,
                            dimNames[0],
                            dimNames[1])
                        .TrackValue(
                            metricValue,
                            dimValues[0],
                            dimValues[1]);
                    break;
                case 3:
                    _telemetryClient
                        .GetMetric(
                            metricName,
                            dimNames[0],
                            dimNames[1],
                            dimNames[2])
                        .TrackValue(
                            metricValue,
                            dimValues[0],
                            dimValues[1],
                            dimValues[2]);
                    break;
                case 4:
                    _telemetryClient
                        .GetMetric(
                            metricName,
                            dimNames[0],
                            dimNames[1],
                            dimNames[2],
                            dimNames[3])
                        .TrackValue(
                            metricValue,
                            dimValues[0],
                            dimValues[1],
                            dimValues[2],
                            dimValues[3]);
                    break;
                case 5:
                    var metric5DId = new MetricIdentifier(
                        _namespace,
                        metricName,
                        dimNames[0],
                        dimNames[1],
                        dimNames[2],
                        dimNames[3],
                        dimNames[4]);

                    _telemetryClient
                        .GetMetric(metric5DId)
                        .TrackValue(
                            metricValue,
                            dimValues[0],
                            dimValues[1],
                            dimValues[2],
                            dimValues[3],
                            dimValues[4]);
                    break;
                case 6:
                    var metric6DId = new MetricIdentifier(
                        _namespace,
                        metricName,
                        dimNames[0],
                        dimNames[1],
                        dimNames[2],
                        dimNames[3],
                        dimNames[4],
                        dimNames[5]);

                    _telemetryClient
                        .GetMetric(metric6DId)
                        .TrackValue(
                            metricValue,
                            dimValues[0],
                            dimValues[1],
                            dimValues[2],
                            dimValues[3],
                            dimValues[4],
                            dimValues[5]);
                    break;
                case 7:
                    var metric7DId = new MetricIdentifier(
                        _namespace,
                        metricName,
                        dimNames[0],
                        dimNames[1],
                        dimNames[2],
                        dimNames[3],
                        dimNames[4],
                        dimNames[5],
                        dimNames[6]);

                    _telemetryClient
                        .GetMetric(metric7DId)
                        .TrackValue(
                            metricValue,
                            dimValues[0],
                            dimValues[1],
                            dimValues[2],
                            dimValues[3],
                            dimValues[4],
                            dimValues[5],
                            dimValues[6]);
                    break;
                case 8:
                    var metric8DId = new MetricIdentifier(
                        _namespace,
                        metricName,
                        dimNames[0],
                        dimNames[1],
                        dimNames[2],
                        dimNames[3],
                        dimNames[4],
                        dimNames[5],
                        dimNames[6],
                        dimNames[7]);

                    _telemetryClient
                        .GetMetric(metric8DId)
                        .TrackValue(
                            metricValue,
                            dimValues[0],
                            dimValues[1],
                            dimValues[2],
                            dimValues[3],
                            dimValues[4],
                            dimValues[5],
                            dimValues[6],
                            dimValues[7]);
                    break;
                case 9:
                    var metric9DId = new MetricIdentifier(
                        _namespace,
                        metricName,
                        dimNames[0],
                        dimNames[1],
                        dimNames[2],
                        dimNames[3],
                        dimNames[4],
                        dimNames[5],
                        dimNames[6],
                        dimNames[7],
                        dimNames[8]);

                    _telemetryClient
                        .GetMetric(metric9DId)
                        .TrackValue(
                            metricValue,
                            dimValues[0],
                            dimValues[1],
                            dimValues[2],
                            dimValues[3],
                            dimValues[4],
                            dimValues[5],
                            dimValues[6],
                            dimValues[7],
                            dimValues[8]);
                    break;
                case 10:
                    var metric10DId = new MetricIdentifier(
                        _namespace,
                        metricName,
                        dimNames[0],
                        dimNames[1],
                        dimNames[2],
                        dimNames[3],
                        dimNames[4],
                        dimNames[5],
                        dimNames[6],
                        dimNames[7],
                        dimNames[8],
                        dimNames[9]);

                    _telemetryClient
                        .GetMetric(metric10DId)
                        .TrackValue(
                            metricValue,
                            dimValues[0],
                            dimValues[1],
                            dimValues[2],
                            dimValues[3],
                            dimValues[4],
                            dimValues[5],
                            dimValues[6],
                            dimValues[7],
                            dimValues[8],
                            dimValues[9]);
                    break;
                default:
                    break;
            }
        }
    }
}
