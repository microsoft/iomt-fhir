// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class EventProcessingMeterTests
    {
        [Fact]
        public async Task GivenEventsArray_CalculateEventStats_ThenEventStatsReturned_Test()
        {
            // 22 bytes
            var body = Encoding.UTF8.GetBytes("22 characters to bytes");
            var evt = new MockEventData(body);

            // 14 bytes
            evt.Properties["7 chars"] = "7 chars";

            var evtBatch = new EventData[] { evt };
            IEventProcessingMeter meter = new EventProcessingMeter();
            var stats = await meter.CalculateEventStats(evtBatch);

            // DateTime.MinValue = "01/01/0001 00:00:00" on Linux
            // DateTime.MinValue = "1/1/0001 12:00:00 AM" on Windows
            if (DateTime.MinValue.ToString() == "01/01/0001 00:00:00")
            {
                Assert.Equal(154, stats.TotalEventsProcessedBytes); // 22 + 118 + 14 = 154
            }
            else
            {
                Assert.Equal(155, stats.TotalEventsProcessedBytes); // 22 + 119 + 14 = 155
            }
        }
    }
}
