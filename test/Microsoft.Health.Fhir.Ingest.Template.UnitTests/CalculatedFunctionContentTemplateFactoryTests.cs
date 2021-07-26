// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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

            var measurementExtractor = template as MeasurementExtractor;
            var expressionTemplate = measurementExtractor.Template;
            var expressionEvaluatorFactory = measurementExtractor.ExpressionEvaluatorFactory;

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
            Assert.Collection(expressionTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.heartrate", v.Value);
                Assert.Null(v.Language);
                Assert.NotNull(expressionEvaluatorFactory.Create(v));
            });
            Assert.NotNull(expressionEvaluatorFactory.Create(expressionTemplate.TypeMatchExpression));
            Assert.NotNull(expressionEvaluatorFactory.Create(expressionTemplate.DeviceIdExpression));
            Assert.NotNull(expressionEvaluatorFactory.Create(expressionTemplate.TimestampExpression));
        }

        [Theory]
        [FileData(@"TestInput/data_CalculatedFunctionContentTemplateValidOptional.json")]
        public void GivenValidTemplateJsonWithOptionalExpressions_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new CalculatedFunctionContentTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var measurementExtractor = template as MeasurementExtractor;
            var expressionTemplate = measurementExtractor.Template;
            var expressionEvaluatorFactory = measurementExtractor.ExpressionEvaluatorFactory;
            Assert.NotNull(expressionTemplate);

            Assert.Equal("heartrate", expressionTemplate.TypeName);
            Assert.Equal("$..[?(@heartrate)]", expressionTemplate.TypeMatchExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, expressionTemplate.TypeMatchExpression.Language);
            Assert.Equal("$.device", expressionTemplate.DeviceIdExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, expressionTemplate.DeviceIdExpression.Language);
            Assert.Equal("$.date", expressionTemplate.TimestampExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, expressionTemplate.TimestampExpression.Language);
            Assert.Equal("$.patientId", expressionTemplate.PatientIdExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, expressionTemplate.PatientIdExpression.Language);
            Assert.Equal("$.correlationId", expressionTemplate.CorrelationIdExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, expressionTemplate.CorrelationIdExpression.Language);
            Assert.Equal("$.encounterId", expressionTemplate.EncounterIdExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, expressionTemplate.CorrelationIdExpression.Language);
            Assert.Collection(expressionTemplate.Values, v =>
            {
                Assert.True(v.Required);
                Assert.Equal("hr", v.ValueName);
                Assert.Equal("$.heartrate", v.Value);
                Assert.Equal(TemplateExpressionLanguage.JsonPath, v.Language);
                Assert.NotNull(expressionEvaluatorFactory.Create(v));
            });
            Assert.NotNull(expressionEvaluatorFactory.Create(expressionTemplate.TypeMatchExpression));
            Assert.NotNull(expressionEvaluatorFactory.Create(expressionTemplate.DeviceIdExpression));
            Assert.NotNull(expressionEvaluatorFactory.Create(expressionTemplate.TimestampExpression));
            Assert.NotNull(expressionEvaluatorFactory.Create(expressionTemplate.PatientIdExpression));
            Assert.NotNull(expressionEvaluatorFactory.Create(expressionTemplate.CorrelationIdExpression));
            Assert.NotNull(expressionEvaluatorFactory.Create(expressionTemplate.EncounterIdExpression));
        }
    }
}
