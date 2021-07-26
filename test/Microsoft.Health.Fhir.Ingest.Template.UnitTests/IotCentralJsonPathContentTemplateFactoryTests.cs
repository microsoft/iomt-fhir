// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class IotCentralJsonPathContentTemplateFactoryTests
    {
        [Theory]
        [FileData(@"TestInput/data_IotCentralJsonPathContentTemplateValid.json")]
        public void GivenValidTemplateJson_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new IotCentralJsonPathContentTemplateFactory();

            var contentTemplate = factory.Create(templateContainer);
            Assert.NotNull(contentTemplate);
            Assert.IsType<LegacyMeasurementExtractor>(contentTemplate);
            var extractor = contentTemplate as MeasurementExtractor;
            Assert.IsType<JsonPathCalculatedFunctionContentTemplateAdapter<IotCentralJsonPathContentTemplate>>(extractor.Template);

            var jsonPathTemplate = (extractor.Template as JsonPathCalculatedFunctionContentTemplateAdapter<IotCentralJsonPathContentTemplate>).InnerTemplate;

            Assert.Equal("telemetry", jsonPathTemplate.TypeName);
            Assert.Equal("$..[?(@telemetry)]", jsonPathTemplate.TypeMatchExpression);
            Assert.Equal("$.deviceId", jsonPathTemplate.DeviceIdExpression);
            Assert.Equal("$.enqueuedTime", jsonPathTemplate.TimestampExpression);
            Assert.Null(jsonPathTemplate.PatientIdExpression);
            Assert.Collection(
                jsonPathTemplate.Values,
                v =>
                {
                    Assert.True(v.Required);
                    Assert.Equal("activity", v.ValueName);
                    Assert.Equal("$.template.Activity", v.ValueExpression);
                },
                v =>
                {
                    Assert.True(v.Required);
                    Assert.Equal("bp_diastolic", v.ValueName);
                    Assert.Equal("$.template.BloodPressure.Diastolic", v.ValueExpression);
                });
        }

        [Theory]
        [FileData(@"TestInput/data_IotCentralJsonPathContentTemplateValidWithOptional.json")]
        public void GivenValidTemplateJsonWithOptionalExpressions_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new IotCentralJsonPathContentTemplateFactory();

            var contentTemplate = factory.Create(templateContainer);
            Assert.NotNull(contentTemplate);
            Assert.IsType<LegacyMeasurementExtractor>(contentTemplate);
            var extractor = contentTemplate as MeasurementExtractor;
            Assert.IsType<JsonPathCalculatedFunctionContentTemplateAdapter<IotCentralJsonPathContentTemplate>>(extractor.Template);

            var jsonPathTemplate = (extractor.Template as JsonPathCalculatedFunctionContentTemplateAdapter<IotCentralJsonPathContentTemplate>).InnerTemplate;

            Assert.Equal("telemetry", jsonPathTemplate.TypeName);
            Assert.Equal("$..[?(@telemetry)]", jsonPathTemplate.TypeMatchExpression);
            Assert.Equal("$.deviceId", jsonPathTemplate.DeviceIdExpression);
            Assert.Equal("$.enqueuedTime", jsonPathTemplate.TimestampExpression);
            Assert.Equal("$.messageProperties.patientId", jsonPathTemplate.PatientIdExpression);
            Assert.Collection(
                jsonPathTemplate.Values,
                v =>
                {
                    Assert.True(v.Required);
                    Assert.Equal("activity", v.ValueName);
                    Assert.Equal("$.template.Activity", v.ValueExpression);
                },
                v =>
                {
                    Assert.True(v.Required);
                    Assert.Equal("bp_diastolic", v.ValueName);
                    Assert.Equal("$.template.BloodPressure.Diastolic", v.ValueExpression);
                });
        }

        [Theory]
        [FileData(@"TestInput/data_IotCentralJsonPathContentTemplateInvalidMissingTypeMetadata.json")]
        public void GivenInvalidTemplateJsonMissingTypeMetadata_WhenFactoryCreate_ThenTemplateErrorReturned_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);
            var factory = new IotCentralJsonPathContentTemplateFactory();

            var ex = Assert.Throws<InvalidTemplateException>(() => factory.Create(templateContainer));
            Assert.NotNull(ex);
            Assert.Contains("TypeName", ex.Message);
            Assert.Contains("TypeMatchExpression", ex.Message);
        }

        [Theory]
        [FileData(@"TestInput/data_IotCentralJsonPathContentTemplateInvalidMissingValueField.json")]
        public void GivenInvalidTemplateJsonMissingValueField_WhenFactoryCreate_ThenTemplateErrorReturned_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);
            var factory = new IotCentralJsonPathContentTemplateFactory();

            var ex = Assert.Throws<InvalidTemplateException>(() => factory.Create(templateContainer));
            Assert.NotNull(ex);
            Assert.Contains("ValueName", ex.Message);
        }

        [Fact]
        public void GivenInvalidTemplateTargetType_WhenFactoryCreate_ThenNullReturned_Test()
        {
            var templateContainer = new TemplateContainer();

            var factory = new IotCentralJsonPathContentTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.Null(template);
        }

        [Fact]
        public void GivenInvalidTemplateBody_WhenFactoryCreate_ThenInvalidTemplateExceptionThrown_Test()
        {
            var templateContainer = new TemplateContainer
            {
                TemplateType = "IotCentralJsonPathContent",
                Template = null,
            };

            var factory = new IotCentralJsonPathContentTemplateFactory();

            var ex = Assert.Throws<InvalidTemplateException>(() => factory.Create(templateContainer));
            Assert.NotNull(ex);
        }
    }
}
