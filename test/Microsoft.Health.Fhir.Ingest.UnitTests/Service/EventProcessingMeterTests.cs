// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Xunit;
using static Microsoft.Azure.EventHubs.EventData;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class EventProcessingMeterTests
    {
        [Fact]
        public async Task GivenEventsArray_CalculateEventStats_ThenEventStatsReturned_Test()
        {
            // 22 bytes
            var body = Encoding.UTF8.GetBytes("22 characters to bytes");
            var evt = new EventData(body);

            // 94 bytes
            evt.SystemProperties = new SystemPropertiesCollection(1, DateTime.MinValue, "1", "1");

            // 14 bytes
            evt.Properties["7 chars"] = "7 chars";

            // 14 bytes
            evt.SystemProperties.TryAdd("7 chars", "7 chars");

            var evtBatch = new EventData[] { evt };
            IEventProcessingMeter meter = new EventProcessingMeter();
            var stats = await meter.CalculateEventStats(evtBatch);

            // 22 + 94 + 14 + 14 = 144
            Assert.Equal(144, stats.TotalEventsProcessedBytes);
        }
    }
}
