// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class EventProcessingMeter : IEventProcessingMeter
    {
        public Task<EventStats> CalculateEventStats(EventData[] events)
        {
            double ingressSizeBytes = 0;
            foreach (var e in events)
            {
                var body = e.Body.Array.Length;

                foreach (KeyValuePair<string, object> entry in e.Properties)
                {
                    ingressSizeBytes = ingressSizeBytes + Encoding.UTF8.GetBytes(entry.Key + entry.Value).Length;
                }

                foreach (KeyValuePair<string, object> entry in e.SystemProperties)
                {
                    ingressSizeBytes = ingressSizeBytes + Encoding.UTF8.GetBytes(entry.Key + entry.Value).Length;
                }
            }

            var eventStats = new EventStats()
            {
                TotalEventsProcessedBytes = ingressSizeBytes,
            };

            return Task.FromResult(eventStats);
        }
    }
}
