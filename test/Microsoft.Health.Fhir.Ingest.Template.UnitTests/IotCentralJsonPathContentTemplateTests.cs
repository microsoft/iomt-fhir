// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class IotCentralJsonPathContentTemplateTests
    {
        private static readonly IContentTemplate BloodPressureTemplate = BuildMeasurementExtractor(new IotCentralJsonPathContentTemplate
        {
            TypeName = "bloodPressure",
            TypeMatchExpression = "$..[?(@telemetry.BloodPressure.Diastolic && @telemetry.BloodPressure.Systolic)]",
            Values = new List<JsonPathValueExpression>
            {
                new JsonPathValueExpression { ValueName = "bp_diastolic", ValueExpression = "$.telemetry.BloodPressure.Diastolic", Required = true },
                new JsonPathValueExpression { ValueName = "bp_systolic", ValueExpression = "$.telemetry.BloodPressure.Systolic", Required = true },
            },
        });

        private static readonly IContentTemplate EnrichmentTemplate = BuildMeasurementExtractor(new IotCentralJsonPathContentTemplate
        {
            TypeName = "elevation",
            TypeMatchExpression = "$..[?(@enrichments.Elevation)]",
            Values = new List<JsonPathValueExpression>
            {
                new JsonPathValueExpression { ValueName = "elevation", ValueExpression = "$.enrichments.Elevation", Required = true },
            },
        });

        private static readonly IContentTemplate TelemetryTemplate = BuildMeasurementExtractor(new IotCentralJsonPathContentTemplate
        {
            TypeName = "telemetry",
            TypeMatchExpression = "$..[?(@telemetry)]",
            Values = new List<JsonPathValueExpression>
            {
                new JsonPathValueExpression { ValueName = "activity", ValueExpression = "$.telemetry.Activity", Required = true },
                new JsonPathValueExpression { ValueName = "bp_diastolic", ValueExpression = "$.telemetry.BloodPressure.Diastolic", Required = true },
                new JsonPathValueExpression { ValueName = "bp_systolic", ValueExpression = "$.telemetry.BloodPressure.Systolic", Required = true },
                new JsonPathValueExpression { ValueName = "respitoryrate", ValueExpression = "$.telemetry.RespiratoryRate", Required = true },
            },
        });

        [Theory]
        [FileData(@"TestInput/data_IotCentralPayloadExample.json")]
        public void GivenBloodpressureTemplate_WhenGetMeasurements_ThenAllMeasurementReturned_Test(string eventJson)
        {
            var token = JsonConvert.DeserializeObject<JToken>(eventJson);
            var result = BloodPressureTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("bloodPressure", m.Type);
                Assert.Equal(token["enqueuedTime"], m.OccurrenceTimeUtc);
                Assert.Equal(token["deviceId"], m.DeviceId);
                Assert.Collection(
                    m.Properties,
                    p =>
                    {
                        Assert.Equal("bp_diastolic", p.Name);
                        Assert.Equal("7", p.Value);
                    },
                    p =>
                    {
                        Assert.Equal("bp_systolic", p.Name);
                        Assert.Equal("71", p.Value);
                    });
            });
        }

        [Theory]
        [FileData(@"TestInput/data_IotCentralPayloadExample.json")]
        public void GivenEnrichmentTemplate_WhenGetMeasurements_ThenAllMeasurementReturned_Test(string eventJson)
        {
            var token = JsonConvert.DeserializeObject<JToken>(eventJson);
            var result = EnrichmentTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("elevation", m.Type);
                Assert.Equal(token["enqueuedTime"], m.OccurrenceTimeUtc);
                Assert.Equal(token["deviceId"], m.DeviceId);
                Assert.Collection(
                    m.Properties,
                    p =>
                    {
                        Assert.Equal("elevation", p.Name);
                        Assert.Equal("200m", p.Value);
                    });
            });
        }

        [Theory]
        [FileData(@"TestInput/data_IotCentralPayloadExample.json")]
        public void GivenTelemetryTemplate_WhenGetMeasurements_ThenAllMeasurementReturned_Test(string eventJson)
        {
            var token = JsonConvert.DeserializeObject<JToken>(eventJson);
            var result = TelemetryTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("telemetry", m.Type);
                Assert.Equal(token["enqueuedTime"], m.OccurrenceTimeUtc);
                Assert.Equal(token["deviceId"], m.DeviceId);
                Assert.Collection(
                    m.Properties,
                    p =>
                    {
                        Assert.Equal("activity", p.Name);
                        Assert.Equal("running", p.Value);
                    },
                    p =>
                    {
                        Assert.Equal("bp_diastolic", p.Name);
                        Assert.Equal("7", p.Value);
                    },
                    p =>
                    {
                        Assert.Equal("bp_systolic", p.Name);
                        Assert.Equal("71", p.Value);
                    },
                    p =>
                    {
                        Assert.Equal("respitoryrate", p.Name);
                        Assert.Equal("13", p.Value);
                    });
            });
        }

        private static IContentTemplate BuildMeasurementExtractor(IotCentralJsonPathContentTemplate template)
        {
            return new LegacyMeasurementExtractor(
                new JsonPathCalculatedFunctionContentTemplateAdapter<IotCentralJsonPathContentTemplate>(template),
                new JsonPathExpressionEvaluatorFactory());
        }
    }
}
