// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public static class EventMetricExtensions
    {
        public static Task<EventStats> CalculateEventStats(this IEnumerable<EventData> events)
        {
            double ingressSizeBytes = 0;

            foreach (var e in events)
            {
                var bodySizeBytes = e.Body.Array.Length;
                var propSizeBytes = EventProcessingMeter.CalculateDictionarySizeBytes(e.Properties);
                var syspSizeBytes = EventProcessingMeter.CalculateDictionarySizeBytes(e.SystemProperties);
                ingressSizeBytes += bodySizeBytes + propSizeBytes + syspSizeBytes;
            }

            var eventStats = new EventStats()
            {
                TotalEventsProcessedBytes = Convert.ToDouble(ingressSizeBytes),
            };

            return Task.FromResult(eventStats);
        }
    }
}
