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
            EnsureArg.IsNotNull(metricName, nameof(metricName));
            MetricName = metricName;
        }

        public string MetricName { get; }

        public static EventMetricDefinition DeviceIngressSizeBytes()
        {
            return new EventMetricDefinition(nameof(DeviceIngressSizeBytes));
        }

        public static EventMetricDefinition MeasurementToFhirBytes()
        {
            return new EventMetricDefinition(nameof(MeasurementToFhirBytes));
        }

        public override string ToString()
        {
            return MetricName;
        }
    }
}
