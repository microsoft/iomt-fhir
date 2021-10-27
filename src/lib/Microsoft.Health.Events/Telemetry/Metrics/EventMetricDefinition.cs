// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Events.Telemetry
{
    public sealed class EventMetricDefinition : MetricDefinition
    {
        private EventMetricDefinition(string metricName)
            : base(metricName)
        {
        }

        public static EventMetricDefinition EventsFlushed { get; } = new EventMetricDefinition(nameof(EventsFlushed));

        public static EventMetricDefinition EventHubChanged { get; } = new EventMetricDefinition(nameof(EventHubChanged));

        public static EventMetricDefinition EventHubPartitionInitialized { get; } = new EventMetricDefinition(nameof(EventHubPartitionInitialized));

        public static EventMetricDefinition EventTimestampLastProcessedPerPartition { get; } = new EventMetricDefinition(nameof(EventTimestampLastProcessedPerPartition));

        public static EventMetricDefinition EventsWatermarkUpdated { get; } = new EventMetricDefinition(nameof(EventsWatermarkUpdated));

        public static EventMetricDefinition DeviceIngressSizeBytes { get; } = new EventMetricDefinition(nameof(DeviceIngressSizeBytes));

        public static EventMetricDefinition MeasurementToFhirBytes { get; } = new EventMetricDefinition(nameof(MeasurementToFhirBytes));
    }
}
