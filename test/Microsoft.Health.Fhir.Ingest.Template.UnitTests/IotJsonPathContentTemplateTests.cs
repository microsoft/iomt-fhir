// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class IotJsonPathContentTemplateTests
    {
        private static readonly IContentTemplate SingleValueTemplate = BuildMeasurementExtractor(new IotJsonPathContentTemplate
        {
            TypeName = "heartrate",
            TypeMatchExpression = "$..[?(@Body.heartrate)]",
            Values = new List<JsonPathValueExpression>
            {
                new JsonPathValueExpression { ValueName = "hr", ValueExpression = "$.Body.heartrate", Required = true },
            },
        });

        private static readonly IContentTemplate MultiValueTemplate = BuildMeasurementExtractor(new IotJsonPathContentTemplate
        {
            TypeName = "bloodpressure",
            TypeMatchExpression = "$..[?(@Body.systolic && @Body.diastolic)]",
            Values = new List<JsonPathValueExpression>
            {
                new JsonPathValueExpression { ValueName = "systolic", ValueExpression = "$.Body.systolic", Required = true },
                new JsonPathValueExpression { ValueName = "diastolic", ValueExpression = "$.Body.diastolic", Required = true },
            },
        });

        [Theory]
        [FileData(@"TestInput/data_IotHubPayloadExample.json")]
        public void GivenTemplateAndSingleValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(string eventJson)
        {
            var token = JsonConvert.DeserializeObject<JToken>(eventJson);
            var result = SingleValueTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(token["Properties"]["iothub-creation-time-utc"], m.OccurrenceTimeUtc);
                Assert.Equal(token["SystemProperties"]["iothub-connection-device-id"], m.DeviceId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("203", p.Value);
                });
            });
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GivenTemplateAndMissingTypeNameToken_WhenGetMeasurements_ThenExceptionIsThrown_Test(string typeMatchExpression)
        {
            var token = JToken.FromObject(new { heartrate = "60", device = "abc" });
            var template = BuildMeasurementExtractor(new IotJsonPathContentTemplate
            {
                TypeName = "heartrate",
                TypeMatchExpression = typeMatchExpression,
                Values = new List<JsonPathValueExpression>
                {
                    new JsonPathValueExpression { ValueName = "hr", ValueExpression = "$.Body.heartrate", Required = true },
                },
            });

            var ex = Assert.Throws<IncompatibleDataException>(() => template.GetMeasurements(token).ToArray());
            Assert.Equal("An expression must be set for [TypeMatchExpression]", ex.Message);
        }

        [Theory]
        [FileData(@"TestInput/data_IotHubPayloadMultiValueExample.json")]
        public void GivenTemplateAndSingleMultiValueValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(string eventJson)
        {
            var token = JsonConvert.DeserializeObject<JToken>(eventJson);
            var result = MultiValueTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("bloodpressure", m.Type);
                Assert.Equal(token["Properties"]["iothub-creation-time-utc"], m.OccurrenceTimeUtc);
                Assert.Equal(token["SystemProperties"]["iothub-connection-device-id"], m.DeviceId);
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
        [FileData(@"TestInput/data_IotHubPayloadExampleMissingCreateTime.json")]
        public void GivenTemplateAndSingleValidTokenWithoutCreationTime_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(string eventJson)
        {
            var token = JsonConvert.DeserializeObject<JToken>(eventJson);
            var result = SingleValueTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(token["SystemProperties"]["iothub-enqueuedtime"], m.OccurrenceTimeUtc);
                Assert.Equal(token["SystemProperties"]["iothub-connection-device-id"], m.DeviceId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("203", p.Value);
                });
            });
        }

        private static IContentTemplate BuildMeasurementExtractor(IotJsonPathContentTemplate template)
        {
            return new IotJsonPathLegacyMeasurementExtractor(template);
        }
    }
}
