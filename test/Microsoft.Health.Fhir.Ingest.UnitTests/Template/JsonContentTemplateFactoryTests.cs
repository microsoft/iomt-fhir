// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class JsonContentTemplateFactoryTests
    {
        [Theory]
        [FileData(@"TestInput/data_JsonPathContentTemplateValid.json")]
        public void GivenValidTemplateJson_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new JsonPathContentTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var jsonPathTemplate = template as JsonPathContentTemplate;
            Assert.NotNull(jsonPathTemplate);

            Assert.Equal("heartrate", jsonPathTemplate.TypeName);
            Assert.Equal("$..[?(@heartrate)]", jsonPathTemplate.TypeMatchExpression);
            Assert.Equal("$.device", jsonPathTemplate.DeviceIdExpression);
            Assert.Equal("$.date", jsonPathTemplate.TimestampExpression);
            Assert.Null(jsonPathTemplate.PatientIdExpression);
            Assert.Collection(jsonPathTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.heartrate", v.ValueExpression);
            });
        }

        [Theory]
        [FileData(@"TestInput/data_JsonPathContentTemplateValidWithOptional.json")]
        public void GivenValidTemplateJsonWithOptionalExpressions_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new JsonPathContentTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var jsonPathTemplate = template as JsonPathContentTemplate;
            Assert.NotNull(jsonPathTemplate);

            Assert.Equal("heartrate", jsonPathTemplate.TypeName);
            Assert.Equal("$..[?(@heartrate)]", jsonPathTemplate.TypeMatchExpression);
            Assert.Equal("$.device", jsonPathTemplate.DeviceIdExpression);
            Assert.Equal("$.date", jsonPathTemplate.TimestampExpression);
            Assert.Equal("$.patient", jsonPathTemplate.PatientIdExpression);
            Assert.Collection(jsonPathTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.heartrate", v.ValueExpression);
            });
        }

        [Fact]
        public void GivenInvalidTemplateTargetType_WhenFactoryCreate_ThenNullReturned_Test()
        {
            var templateContainer = new TemplateContainer();

            var factory = new JsonPathContentTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.Null(template);
        }

        [Fact]
        public void GivenInvalidTemplateBody_WhenFactoryCreate_ThenInvalidTemplateExceptionThrown_Test()
        {
            var templateContainer = new TemplateContainer
            {
                TemplateType = "JsonPathContentTemplate",
                Template = null,
            };

            var factory = new JsonPathContentTemplateFactory();

            var ex = Assert.Throws<InvalidTemplateException>(() => factory.Create(templateContainer));
            Assert.NotNull(ex);
        }
    }
}
