// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Template;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class JsonPathCalculatedFunctionContentTemplateAdapterTests
    {
        [Fact]
        public void When_AllExpressionsAreSupplied_AllExpressions_Are_Initialized()
        {
            var contentTemplate = new JsonPathContentTemplate
            {
                TypeName = "heartrate",
                TypeMatchExpression = "$..[?(@heartrate)]",
                DeviceIdExpression = "$.device",
                TimestampExpression = "$.date",
                PatientIdExpression = "$.patientId",
                CorrelationIdExpression = "$.corId",
                EncounterIdExpression = "$.encounterId",
                Values = new List<JsonPathValueExpression>
                {
                  new JsonPathValueExpression { ValueName = "hr1", ValueExpression = "$.heartrate", Required = false },
                  new JsonPathValueExpression { ValueName = "hr2", ValueExpression = "$.heartrate2", Required = true },
                },
            };

            var facade = new JsonPathCalculatedFunctionContentTemplateAdapter<JsonPathContentTemplate>(contentTemplate);

            Assert.Equal("heartrate", facade.TypeName);
            Assert.Equal("$..[?(@heartrate)]", facade.TypeMatchExpression.Value);
            Assert.Equal("$.device", facade.DeviceIdExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, facade.DeviceIdExpression.Language);
            Assert.Equal("$.date", facade.TimestampExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, facade.TimestampExpression.Language);
            Assert.Equal("$.patientId", facade.PatientIdExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, facade.PatientIdExpression.Language);
            Assert.Equal("$.corId", facade.CorrelationIdExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, facade.CorrelationIdExpression.Language);
            Assert.Equal("$.encounterId", facade.EncounterIdExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, facade.EncounterIdExpression.Language);
            Assert.Collection(
                facade.Values,
                item =>
                {
                    Assert.Equal("hr1", item.ValueName);
                    Assert.Equal("$.heartrate", item.ValueExpression.Value);
                    Assert.False(item.Required);
                },
                item =>
                {
                    Assert.Equal("hr2", item.ValueName);
                    Assert.Equal("$.heartrate2", item.ValueExpression.Value);
                    Assert.True(item.Required);
                });
        }

        [Fact]
        public void When_ExpressionsAreNotSupplied_MissingExpressionsAreNotInitialized()
        {
            var contentTemplate = new JsonPathContentTemplate
            {
                TypeName = "heartrate",
                TypeMatchExpression = "$..[?(@heartrate)]",
                DeviceIdExpression = "$.device",
                TimestampExpression = "$.date",
                Values = new List<JsonPathValueExpression>
                {
                  new JsonPathValueExpression { ValueName = "hr1", ValueExpression = "$.heartrate", Required = false },
                  new JsonPathValueExpression { ValueName = "hr2", ValueExpression = "$.heartrate2", Required = true },
                },
            };

            var facade = new JsonPathCalculatedFunctionContentTemplateAdapter<JsonPathContentTemplate>(contentTemplate);

            Assert.Equal("heartrate", facade.TypeName);
            Assert.Equal("$..[?(@heartrate)]", facade.TypeMatchExpression.Value);
            Assert.Equal("$.device", facade.DeviceIdExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, facade.DeviceIdExpression.Language);
            Assert.Equal("$.date", facade.TimestampExpression.Value);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, facade.TimestampExpression.Language);
            Assert.Null(facade.PatientIdExpression);
            Assert.Null(facade.CorrelationIdExpression);
            Assert.Null(facade.EncounterIdExpression);
            Assert.Collection(
                facade.Values,
                item =>
                {
                    Assert.Equal("hr1", item.ValueName);
                    Assert.Equal("$.heartrate", item.ValueExpression.Value);
                    Assert.False(item.Required);
                },
                item =>
                {
                    Assert.Equal("hr2", item.ValueName);
                    Assert.Equal("$.heartrate2", item.ValueExpression.Value);
                    Assert.True(item.Required);
                });
        }
    }
}
