// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;

namespace Microsoft.Health.Events.Telemetry
{
    public sealed class EventMetricDefinition
    {
        private EventMetricDefinition(string metricName)
        {
            EnsureArg.IsNotNullOrEmpty(metricName, nameof(metricName));
            MetricName = metricName;
        }

        public string MetricName { get; }

        public static EventMetricDefinition EventsFlushed { get; } = new EventMetricDefinition(nameof(EventsFlushed));

        public static EventMetricDefinition EventHubChanged { get; } = new EventMetricDefinition(nameof(EventHubChanged));

        public static EventMetricDefinition EventHubPartitionInitialized { get; } = new EventMetricDefinition(nameof(EventHubPartitionInitialized));

        public static EventMetricDefinition EventTimestampLastProcessedPerPartition { get; } = new EventMetricDefinition(nameof(EventTimestampLastProcessedPerPartition));

        public static EventMetricDefinition EventsWatermarkUpdated { get; } = new EventMetricDefinition(nameof(EventsWatermarkUpdated));

        public static EventMetricDefinition DeviceIngressSizeBytes { get; } = new EventMetricDefinition(nameof(DeviceIngressSizeBytes));

        public static EventMetricDefinition MeasurementToFhirBytes { get; } = new EventMetricDefinition(nameof(MeasurementToFhirBytes));

        public override string ToString()
        {
            return MetricName;
        }
    }
}
