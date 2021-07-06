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
        [FileData(@"TestInput/data_CalculatedFunctionContentTemplateValidExpressionObject.json")]
        public void GivenValidTemplateJson_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new CalculatedFunctionContentTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var expressionTemplate = template as CalculatedFunctionContentTemplate;
            Assert.NotNull(expressionTemplate);

            Assert.Equal("heartrate", expressionTemplate.TypeName);
            Assert.Equal("$..[?(@heartrate)]", expressionTemplate.TypeMatchExpression.Value);
            Assert.Null(expressionTemplate.TypeMatchExpression.Language);
            Assert.Equal("$.device", expressionTemplate.DeviceIdExpression.Value);
            Assert.Null(expressionTemplate.DeviceIdExpression.Language);
            Assert.Equal("$.date", expressionTemplate.TimestampExpression.Value);
            Assert.Null(expressionTemplate.TimestampExpression.Language);
            Assert.Null(expressionTemplate.PatientIdExpression);
            Assert.Null(expressionTemplate.CorrelationIdExpression);
            Assert.IsType<CachingExpressionEvaluatorFactory>(expressionTemplate.ExpressionEvaluatorFactory);
            Assert.Collection(expressionTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.heartrate", v.Value);
                Assert.Null(v.Language);
                Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(v, ExpressionLanguage.JmesPath));
            });
            Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(expressionTemplate.TypeMatchExpression, ExpressionLanguage.JmesPath));
            Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(expressionTemplate.DeviceIdExpression, ExpressionLanguage.JmesPath));
            Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(expressionTemplate.TimestampExpression, ExpressionLanguage.JmesPath));
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
            Assert.Equal("$..[?(@heartrate)]", expressionTemplate.TypeMatchExpression.Value);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.TypeMatchExpression.Language);
            Assert.Equal("$.device", expressionTemplate.DeviceIdExpression.Value);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.DeviceIdExpression.Language);
            Assert.Equal("$.date", expressionTemplate.TimestampExpression.Value);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.TimestampExpression.Language);
            Assert.Equal("$.patientId", expressionTemplate.PatientIdExpression.Value);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.PatientIdExpression.Language);
            Assert.Equal("$.correlationId", expressionTemplate.CorrelationIdExpression.Value);
            Assert.Equal(ExpressionLanguage.JsonPath, expressionTemplate.CorrelationIdExpression.Language);
            Assert.IsType<CachingExpressionEvaluatorFactory>(expressionTemplate.ExpressionEvaluatorFactory);
            Assert.Collection(expressionTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.heartrate", v.Value);
                Assert.Equal(ExpressionLanguage.JsonPath, v.Language);
                Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(v, ExpressionLanguage.JmesPath));
            });
            Assert.IsType<CachingExpressionEvaluatorFactory>(expressionTemplate.ExpressionEvaluatorFactory);
            Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(expressionTemplate.TypeMatchExpression, ExpressionLanguage.JmesPath));
            Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(expressionTemplate.DeviceIdExpression, ExpressionLanguage.JmesPath));
            Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(expressionTemplate.TimestampExpression, ExpressionLanguage.JmesPath));
            Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(expressionTemplate.PatientIdExpression, ExpressionLanguage.JmesPath));
            Assert.NotNull(expressionTemplate.ExpressionEvaluatorFactory.Create(expressionTemplate.CorrelationIdExpression, ExpressionLanguage.JmesPath));
        }
    }
}
