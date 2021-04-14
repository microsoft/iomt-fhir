// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class EventProcessingMeter : IEventProcessingMeter
    {
        public Task<EventStats> CalculateEventStats(IEnumerable<EventData> events)
        {
            double ingressSizeBytes = 0;

            foreach (var e in events)
            {
                var bodySizeBytes = e.Body.Array.Length;
                ingressSizeBytes = ingressSizeBytes + bodySizeBytes;
                ingressSizeBytes = e.Properties.Aggregate(ingressSizeBytes, (current, entry) => current + Encoding.UTF8.GetByteCount(entry.Key) + Encoding.UTF8.GetByteCount(entry.Value.ToString()));
                ingressSizeBytes = e.SystemProperties.Aggregate(ingressSizeBytes, (current, entry) => current + Encoding.UTF8.GetByteCount(entry.Key) + Encoding.UTF8.GetByteCount(entry.Value.ToString()));
            }

            var eventStats = new EventStats()
            {
                TotalEventsProcessedBytes = ingressSizeBytes,
            };

            return Task.FromResult(eventStats);
        }
    }
}
