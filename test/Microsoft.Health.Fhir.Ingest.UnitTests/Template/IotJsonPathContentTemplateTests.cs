// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Tests.Common;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class IotJsonPathContentTemplateTests
    {
        private static readonly IContentTemplate SingleValueTemplate = new IotJsonPathContentTemplate
        {
            TypeName = "heartrate",
            TypeMatchExpression = "$..[?(@Body.heartrate)]",
            Values = new List<JsonPathValueExpression>
            {
                new JsonPathValueExpression { ValueName = "hr", ValueExpression = "$.Body.heartrate", Required = true },
            },
        };

        private static readonly IContentTemplate MultiValueTemplate = new IotJsonPathContentTemplate
        {
            TypeName = "bloodpressure",
            TypeMatchExpression = "$..[?(@Body.systolic && @Body.diastolic)]",
            Values = new List<JsonPathValueExpression>
            {
                new JsonPathValueExpression { ValueName = "systolic", ValueExpression = "$.Body.systolic", Required = true },
                new JsonPathValueExpression { ValueName = "diastolic", ValueExpression = "$.Body.diastolic", Required = true },
            },
        };

        [Theory]
        [FileData(@"TestInput/data_IotHubPayloadExample.json")]
        public void GivenTemplateAndSingleValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(string eventJson)
        {
            var evt = EventDataTestHelper.BuildEventFromJson(eventJson);
            var token = new EventDataWithJsonBodyToJTokenConverter().Convert(evt);

            var result = SingleValueTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(evt.Properties["iothub-creation-time-utc"], m.OccurrenceTimeUtc);
                Assert.Equal(evt.SystemProperties["iothub-connection-device-id"], m.DeviceId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("203", p.Value);
                });
            });
        }

        [Theory]
        [FileData(@"TestInput/data_IotHubPayloadMultiValueExample.json")]
        public void GivenTemplateAndSingleMultiValueValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(string eventJson)
        {
            var evt = EventDataTestHelper.BuildEventFromJson(eventJson);
            var token = new EventDataWithJsonBodyToJTokenConverter().Convert(evt);

            var result = MultiValueTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("bloodpressure", m.Type);
                Assert.Equal(evt.Properties["iothub-creation-time-utc"], m.OccurrenceTimeUtc);
                Assert.Equal(evt.SystemProperties["iothub-connection-device-id"], m.DeviceId);
                Assert.Collection(
                    m.Properties,
                    p =>
                    {
                        Assert.Equal("systolic", p.Name);
                        Assert.Equal("111", p.Value);
                    },
                    p =>
                    {
                        Assert.Equal("diastolic", p.Name);
                        Assert.Equal("75", p.Value);
                    });
            });
        }

        [Theory]
        [FileData(@"TestInput/data_IoTHubPayloadExampleMissingCreateTime.json")]
        public void GivenTemplateAndSingleValidTokenWithoutCreationTime_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(string eventJson)
        {
            var evt = EventDataTestHelper.BuildEventFromJson(eventJson);
            var token = new EventDataWithJsonBodyToJTokenConverter().Convert(evt);

            var result = SingleValueTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(evt.SystemProperties["iothub-enqueuedtime"], m.OccurrenceTimeUtc);
                Assert.Equal(evt.SystemProperties["iothub-connection-device-id"], m.DeviceId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("203", p.Value);
                });
            });
        }
    }
}
