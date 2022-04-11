// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.Common
{
    public class EventProcessingMetricMeters : IEventProcessingMetricMeters
    {
        private IEnumerable<IEventProcessingMeter> _eventProcessingMeters;

        public EventProcessingMetricMeters(IEnumerable<IEventProcessingMeter> eventProcessingMeters)
        {
            _eventProcessingMeters = eventProcessingMeters;
        }

        public async Task<IEnumerable<KeyValuePair<Metric, double>>> GetMetrics(IEnumerable<IEventMessage> events)
        {
            var metrics = new List<KeyValuePair<Metric, double>>();

            foreach (var e in _eventProcessingMeters)
            {
                metrics.Add(await e.GetMetric(events));
            }

            return metrics;
        }
    }
}
