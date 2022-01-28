// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Metrics;

namespace Microsoft.Health.Logging.Telemetry
{
    public static class TelemetryExtensions
    {
        private static readonly string _namespace = MetricIdentifier.DefaultMetricNamespace;

        public static void LogException(this TelemetryClient telemetryClient, Exception ex)
        {
            EnsureArg.IsNotNull(telemetryClient, nameof(telemetryClient));
            EnsureArg.IsNotNull(ex, nameof(ex));

            var exceptionTelemetry = new ExceptionTelemetry(ex);

            exceptionTelemetry.Properties.Add("message", ex.Message ?? string.Empty);
            exceptionTelemetry.Properties.Add("helpLink", ex.HelpLink ?? string.Empty);

            telemetryClient.TrackException(exceptionTelemetry);
        }

        public static void LogMetric(this TelemetryClient telemetryClient, Common.Telemetry.Metric metric, double metricValue)
        {
            EnsureArg.IsNotNull(telemetryClient);
            EnsureArg.IsNotNull(metric);

            var metricName = metric.Name;
            var dimensions = metric.Dimensions;
            var dimensionNumber = metric.Dimensions.Count;

            if (dimensionNumber > 10)
            {
                telemetryClient.TrackException(
                    new Exception($"Metric {metricName} exceeds the amount of allowed dimensions"));
                return;
            }

            string[] dimNames = new string[dimensions.Count];
            dimensions.Keys.CopyTo(dimNames, 0);

            string[] dimValues = new string[dimNames.Length];
            int count = 0;
            foreach (string dimName in dimNames)
            {
                dimValues[count] = dimensions[dimName].ToString();
                count++;
            }

            dimensions.Values.CopyTo(dimValues, 0);

            switch (dimensionNumber)
            {
                case 0:
                    telemetryClient
                        .GetMetric(
                            metricName)
                        .TrackValue(
                            metricValue);
                    break;
                case 1:
                    telemetryClient
                        .GetMetric(
                            metricName,
                            dimNames[0])
                        .TrackValue(
                            metricValue,
                            dimValues[0]);
                    break;
                case 2:
                    telemetryClient
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
                    telemetryClient
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
                    telemetryClient
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

                    telemetryClient
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

                    telemetryClient
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

                    telemetryClient
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

                    telemetryClient
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

                    telemetryClient
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

                    telemetryClient
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
