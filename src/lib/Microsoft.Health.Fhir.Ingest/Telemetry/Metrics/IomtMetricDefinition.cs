// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public sealed class IomtMetricDefinition : MetricDefinition
    {
        private IomtMetricDefinition(string metricName)
            : base(metricName)
        {
        }

        public static IomtMetricDefinition DeviceEvent { get; } = new IomtMetricDefinition(nameof(DeviceEvent));

        public static IomtMetricDefinition DeviceEventProcessingLatency { get; } = new IomtMetricDefinition(nameof(DeviceEventProcessingLatency));

        public static IomtMetricDefinition DeviceEventProcessingLatencyMs { get; } = new IomtMetricDefinition(nameof(DeviceEventProcessingLatencyMs));

        public static IomtMetricDefinition DeviceIngressSizeBytes { get; } = new IomtMetricDefinition(nameof(DeviceIngressSizeBytes));

        public static IomtMetricDefinition NormalizedEvent { get; } = new IomtMetricDefinition(nameof(NormalizedEvent));

        public static IomtMetricDefinition Measurement { get; } = new IomtMetricDefinition(nameof(Measurement));

        public static IomtMetricDefinition MeasurementGroup { get; } = new IomtMetricDefinition(nameof(MeasurementGroup));

        public static IomtMetricDefinition MeasurementIngestionLatency { get; } = new IomtMetricDefinition(nameof(MeasurementIngestionLatency));

        public static IomtMetricDefinition MeasurementIngestionLatencyMs { get; } = new IomtMetricDefinition(nameof(MeasurementIngestionLatencyMs));
    }
}
