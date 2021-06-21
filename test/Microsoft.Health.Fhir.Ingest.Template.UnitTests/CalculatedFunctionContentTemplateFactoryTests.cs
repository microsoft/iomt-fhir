// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CalculatedFunctionContentTemplateFactoryTests
    {
        [Theory]
        [FileData(@"TestInput/data_CalculatedFunctionContentTemplateValid.json")]
        public void GivenValidTemplateJson_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new CalculatedFunctionContentTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var expressionTemplate = template as CalculatedFunctionContentTemplate;
            Assert.NotNull(expressionTemplate);

            Assert.Equal("heartrate", expressionTemplate.TypeName);
            Assert.Equal("$..[?(@heartrate)]", expressionTemplate.TypeMatchExpression);
            Assert.Null(expressionTemplate.TypeMatchExpressionLanguage);
            Assert.Equal("$.device", expressionTemplate.DeviceIdExpression);
            Assert.Null(expressionTemplate.DeviceIdExpressionLanguage);
            Assert.Equal("$.date", expressionTemplate.TimestampExpression);
            Assert.Null(expressionTemplate.TimestampExpressionLanguage);
            Assert.Null(expressionTemplate.PatientIdExpression);
            Assert.Null(expressionTemplate.PatientIdExpressionLanguage);
            Assert.Null(expressionTemplate.CorrelationIdExpression);
            Assert.Null(expressionTemplate.CorrelationIdExpressionLanguage);
            Assert.Collection(expressionTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.heartrate", v.ValueExpression);
                Assert.Null(v.ValueExpressionLanguage);
            });
        }

        [Theory]
        [FileData(@"TestInput/data_CalculatedFunctionContentTemplateValidOptional.json")]
        public void GivenValidTemplateJsonWithOptionalExpressions_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new CalculatedFunctionContentTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var expressionTemplate = template as CalculatedFunctionContentTemplate;
            Assert.NotNull(expressionTemplate);

            Assert.Equal("heartrate", expressionTemplate.TypeName);
            Assert.Equal("$..[?(@heartrate)]", expressionTemplate.TypeMatchExpression);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.TypeMatchExpressionLanguage);
            Assert.Equal("$.device", expressionTemplate.DeviceIdExpression);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.DeviceIdExpressionLanguage);
            Assert.Equal("$.date", expressionTemplate.TimestampExpression);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.TimestampExpressionLanguage);
            Assert.Equal("$.patientId", expressionTemplate.PatientIdExpression);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.PatientIdExpressionLanguage);
            Assert.Equal("$.correlationId", expressionTemplate.CorrelationIdExpression);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.CorrelationIdExpressionLanguage);
            Assert.Collection(expressionTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.heartrate", v.ValueExpression);
                Assert.Equal(ExpressionLanguage.JsonPath, v.ValueExpressionLanguage);
            });
        }
    }
}
