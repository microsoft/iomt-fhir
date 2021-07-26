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
    public class IotJsonPathContentTemplateFactoryTests
    {
        [Theory]
        [FileData(@"TestInput/data_IotJsonPathContentTemplateValid.json")]
        public void GivenValidTemplateJson_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new IotJsonPathContentTemplateFactory();

            var contentTemplate = factory.Create(templateContainer);
            Assert.NotNull(contentTemplate);
            Assert.IsType<IotJsonPathLegacyMeasurementExtractor>(contentTemplate);
            var extractor = contentTemplate as MeasurementExtractor;
            Assert.IsType<JsonPathCalculatedFunctionContentTemplateAdapter<IotJsonPathContentTemplate>>(extractor.Template);

            var jsonPathTemplate = (extractor.Template as JsonPathCalculatedFunctionContentTemplateAdapter<IotJsonPathContentTemplate>).InnerTemplate;

            Assert.Equal("heartrate", jsonPathTemplate.TypeName);
            Assert.Equal("$..[?(@Body.heartrate)]", jsonPathTemplate.TypeMatchExpression);
            Assert.Equal("$.SystemProperties.iothub-connection-device-id", jsonPathTemplate.DeviceIdExpression);
            Assert.Equal("$.Properties.iothub-creation-time-utc", jsonPathTemplate.TimestampExpression);
            Assert.Null(jsonPathTemplate.PatientIdExpression);
            Assert.Collection(jsonPathTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.Body.heartrate", v.ValueExpression);
            });
        }

        [Theory]
        [FileData(@"TestInput/data_IotJsonPathContentTemplateValidWithOptional.json")]
        public void GivenValidTemplateJsonWithOptionalExpressions_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new IotJsonPathContentTemplateFactory();

            var contentTemplate = factory.Create(templateContainer);
            Assert.NotNull(contentTemplate);
            Assert.IsType<IotJsonPathLegacyMeasurementExtractor>(contentTemplate);
            var extractor = contentTemplate as MeasurementExtractor;
            Assert.IsType<JsonPathCalculatedFunctionContentTemplateAdapter<IotJsonPathContentTemplate>>(extractor.Template);

            var jsonPathTemplate = (extractor.Template as JsonPathCalculatedFunctionContentTemplateAdapter<IotJsonPathContentTemplate>).InnerTemplate;

            Assert.Equal("heartrate", jsonPathTemplate.TypeName);
            Assert.Equal("$..[?(@Body.heartrate)]", jsonPathTemplate.TypeMatchExpression);
            Assert.Equal("$.SystemProperties.iothub-connection-device-id", jsonPathTemplate.DeviceIdExpression);
            Assert.Equal("$.Properties.iothub-creation-time-utc", jsonPathTemplate.TimestampExpression);
            Assert.Equal("$.patient", jsonPathTemplate.PatientIdExpression);
            Assert.Collection(jsonPathTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.Body.heartrate", v.ValueExpression);
            });
        }

        [Theory]
        [FileData(@"TestInput/data_IotJsonPathContentTemplateInvalidMissingTypeMetadata.json")]
        public void GivenInvalidTemplateJsonMissingTypeMetadata_WhenFactoryCreate_ThenTemplateErrorReturned_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);
            var factory = new IotJsonPathContentTemplateFactory();

            var ex = Assert.Throws<InvalidTemplateException>(() => factory.Create(templateContainer));
            Assert.NotNull(ex);
            Assert.Contains("TypeName", ex.Message);
            Assert.Contains("TypeMatchExpression", ex.Message);
        }

        [Theory]
        [FileData(@"TestInput/data_IotJsonPathContentTemplateInvalidMissingValueField.json")]
        public void GivenInvalidTemplateJsonMissingValueField_WhenFactoryCreate_ThenTemplateErrorReturned_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);
            var factory = new IotJsonPathContentTemplateFactory();

            var ex = Assert.Throws<InvalidTemplateException>(() => factory.Create(templateContainer));
            Assert.NotNull(ex);
            Assert.Contains("ValueName", ex.Message);
        }

        [Fact]
        public void GivenInvalidTemplateTargetType_WhenFactoryCreate_ThenNullReturned_Test()
        {
            var templateContainer = new TemplateContainer();

            var factory = new IotJsonPathContentTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.Null(template);
        }

        [Fact]
        public void GivenInvalidTemplateBody_WhenFactoryCreate_ThenInvalidTemplateExceptionThrown_Test()
        {
            var templateContainer = new TemplateContainer
            {
                TemplateType = "IotJsonPathContentTemplate",
                Template = null,
            };

            var factory = new IotJsonPathContentTemplateFactory();

            var ex = Assert.Throws<InvalidTemplateException>(() => factory.Create(templateContainer));
            Assert.NotNull(ex);
        }
    }
}
