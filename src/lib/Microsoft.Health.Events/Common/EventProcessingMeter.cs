// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.Common
{
    public class EventProcessingMeter : IEventProcessingMeter
    {
        private ITelemetryLogger _logger;

        public EventProcessingMeter()
        {
        }

        public EventProcessingMeter(ITelemetryLogger logger, Metric metric)
        {
            _logger = logger;
            EventsProcessedMetric = metric;
        }

        public Metric EventsProcessedMetric { get; set; }

        public Task<EventStats> CalculateEventStats(IEnumerable<IEventMessage> events)
        {
            double totalBytes = 0;

            foreach (var e in events)
            {
                var bodySizeBytes = e.Body.Length;
                totalBytes = totalBytes + bodySizeBytes + CalculateDictionarySizeBytes(e.Properties) + CalculateDictionarySizeBytes(e.SystemProperties);
            }

            var eventStats = new EventStats()
            {
                TotalEventsProcessedBytes = totalBytes,
            };

            return Task.FromResult(eventStats);
        }

        private double CalculateDictionarySizeBytes(IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            double bytes = dictionary.Aggregate(0, (current, entry) => current + Encoding.UTF8.GetByteCount(entry.Key) + Encoding.UTF8.GetByteCount(entry.Value.ToString()));
            return bytes;
        }

        public void LogEventsProcessedMetric(EventStats eventStats)
        {
            _logger.LogMetric(EventsProcessedMetric, eventStats.TotalEventsProcessedBytes);
        }
    }
}
