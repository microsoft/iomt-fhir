// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class EventProcessingMeter : IEventProcessingMeter
    {
        public Task<EventStats> CalculateEventStats(IEnumerable<EventData> events)
        {
            double ingressSizeBytes = 0;

            foreach (var e in events)
            {
                var bodySizeBytes = e.Body.Length;
                var propSizeBytes = CalculateDictionarySizeBytes(e.Properties);
                var syspSizeBytes = CalculateDictionarySizeBytes(e.SystemProperties);
                ingressSizeBytes += bodySizeBytes + propSizeBytes + syspSizeBytes;
            }

            var eventStats = new EventStats()
            {
                TotalEventsProcessedBytes = Convert.ToDouble(ingressSizeBytes),
            };

            return Task.FromResult(eventStats);
        }

        private int CalculateDictionarySizeBytes(IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            int bytes = dictionary.Aggregate(0, (current, entry) => current + Encoding.UTF8.GetByteCount(entry.Key) + Encoding.UTF8.GetByteCount(entry.Value.ToString()));
            return bytes;
        }
    }
}
