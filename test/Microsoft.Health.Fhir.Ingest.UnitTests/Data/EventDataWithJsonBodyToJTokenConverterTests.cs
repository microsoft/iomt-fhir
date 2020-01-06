// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.Azure.EventHubs;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class EventDataWithJsonBodyToJTokenConverterTests
    {
        [Fact]
        public void GivenEmptyEvent_WhenConvert_ThenTokenReturned_Test()
        {
            var evt = new EventData(Array.Empty<byte>());

            var token = new EventDataWithJsonBodyToJTokenConverter().Convert(evt);

            Assert.NotNull(token);
            Assert.NotNull(token["Body"]);
            Assert.NotNull(token["Properties"]);
            Assert.NotNull(token["SystemProperties"]);
        }

        [Fact]
        public void GivenPopulatedEvent_WhenConvert_ThenTokenWithNonSerializedBodyReturned_Test()
        {
            var bodyObj = new { p1 = 1, p2 = "a", p3 = new DateTime(2019, 01, 01, 12, 30, 20) };
            var currentTime = DateTime.UtcNow;
            var offset = Guid.NewGuid().ToString();
            var partitionKey = Guid.NewGuid().ToString();

            var evt = new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bodyObj)));
            evt.Properties.Add("a", 10);
            evt.SystemProperties = new EventData.SystemPropertiesCollection(100, currentTime, offset, partitionKey);
            var token = new EventDataWithJsonBodyToJTokenConverter().Convert(evt);

            Assert.NotNull(token);
            Assert.NotNull(token["Body"]);
            Assert.NotNull(token["Properties"]);
            Assert.NotNull(token["SystemProperties"]);

            // Verify Body is correctly turned into a JToken instead of a base 64 encoded array
            Assert.Equal(bodyObj.p1.ToString(), token["Body"]["p1"].ToString());
            Assert.Equal(bodyObj.p2.ToString(), token["Body"]["p2"].ToString());
            Assert.Equal(bodyObj.p3.ToString(), token["Body"]["p3"].ToString());

            // Verify EventData.Properties are present on the returned JToken
            Assert.Equal(10.ToString(), token["Properties"]["a"].ToString());

            // Verify EventData.SystemProperties are present on the returned JToken
            Assert.Equal(100.ToString(), token["SystemProperties"]["x-opt-sequence-number"].ToString());
            Assert.Equal(currentTime.ToString(), token["SystemProperties"]["x-opt-enqueued-time"].ToString());
            Assert.Equal(offset.ToString(), token["SystemProperties"]["x-opt-offset"].ToString());
            Assert.Equal(partitionKey.ToString(), token["SystemProperties"]["x-opt-partition-key"].ToString());
        }

        [Theory]
        [FileData(@"TestInput/data_IotHubPayloadExample.json")]
        public void GivenIoTCentralPopulatedEvent_WhenConvert_ThenTokenWithNonSerializedBodyAndPropertiesReturned_Test(string json)
        {
            var evt = EventDataTestHelper.BuildEventFromJson(json);

            var token = new EventDataWithJsonBodyToJTokenConverter().Convert(evt);

            Assert.NotNull(token);
            Assert.NotNull(token["Body"]);
            Assert.NotNull(token["Properties"]);
            Assert.NotNull(token["SystemProperties"]);

            // Verify Body is correctly turned into a JToken instead of a base 64 encoded array
            Assert.Equal("203", token["Body"]["heartrate"].ToString());

            // Verify EventData.Properties are present on the returned JToken
            Assert.Equal("2019-01-30T22:45:02.6073744Z", token["Properties"]["iothub-creation-time-utc"].ToObject<DateTime>().ToString("o"));
            Assert.Equal("America/Los_Angeles", token["Properties"]["tz"].ToString());
            Assert.Equal("55b1e26f-9c83-4264-a0dd-3567afd633d6", token["Properties"]["batchid"].ToString());
            Assert.Equal("60", token["Properties"]["batchsize"].ToString());

            // Verify EventData.SystemProperties are present on the returned JToken
            Assert.Equal("ev-d795-1d04-55ae", token["SystemProperties"]["iothub-connection-device-id"].ToString());
            Assert.Equal("{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}", token["SystemProperties"]["iothub-connection-auth-method"].ToString());
            Assert.Equal("636845741198574895", token["SystemProperties"]["iothub-connection-auth-generation-id"].ToString());
            Assert.Equal("2019-02-01T22:46:01.8750000Z", token["SystemProperties"]["iothub-enqueuedtime"].ToObject<DateTime>().ToString("o"));
        }
    }
}
