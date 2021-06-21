// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CalculatedFunctionContentTemplateConfigurationTests
    {
        [Fact]
        public void Given_NoTemplateLanguage_And_DefaultLanguaged_NotSpecified_JsonPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = "$..[?(@heartrate)]",
                    DeviceIdExpression = "$.device",
                    TimestampExpression = "$.date",
                    CorrelationIdExpression = "$.session",
                    PatientIdExpression = "$.patient",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = "$.matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_NoTemplateLanguage_And_DefaultLanguaged_Specified_JsonPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = ExpressionLanguage.JsonPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = "$..[?(@heartrate)]",
                    DeviceIdExpression = "$.device",
                    TimestampExpression = "$.date",
                    CorrelationIdExpression = "$.session",
                    PatientIdExpression = "$.patient",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = "$.matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_TemplateLanguage_And_DefaultLanguaged_NotSpecified_JsonPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = "$..[?(@heartrate)]",
                    TypeMatchExpressionLanguage = ExpressionLanguage.JsonPath,
                    DeviceIdExpression = "$.device",
                    DeviceIdExpressionLanguage = ExpressionLanguage.JsonPath,
                    TimestampExpression = "$.date",
                    TimestampExpressionLanguage = ExpressionLanguage.JsonPath,
                    CorrelationIdExpression = "$.session",
                    CorrelationIdExpressionLanguage = ExpressionLanguage.JsonPath,
                    PatientIdExpression = "$.patient",
                    PatientIdExpressionLanguage = ExpressionLanguage.JsonPath,
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false,  ValueExpressionLanguage = ExpressionLanguage.JsonPath },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = "$.matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_TemplateLanguage_And_DefaultLanguaged_Specified_JsonPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = ExpressionLanguage.JsonPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = "$..[?(@heartrate)]",
                    TypeMatchExpressionLanguage = ExpressionLanguage.JsonPath,
                    DeviceIdExpression = "$.device",
                    DeviceIdExpressionLanguage = ExpressionLanguage.JsonPath,
                    TimestampExpression = "$.date",
                    TimestampExpressionLanguage = ExpressionLanguage.JsonPath,
                    CorrelationIdExpression = "$.session",
                    CorrelationIdExpressionLanguage = ExpressionLanguage.JsonPath,
                    PatientIdExpression = "$.patient",
                    PatientIdExpressionLanguage = ExpressionLanguage.JsonPath,
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false,  ValueExpressionLanguage = ExpressionLanguage.JsonPath },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = "$.matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_NoTemplateLanguage_And_DefaultLanguaged_Specified_JMESPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = "to_array(@)[?heartrate]",
                    DeviceIdExpression = "device",
                    TimestampExpression = "date",
                    CorrelationIdExpression = "session",
                    PatientIdExpression = "patient",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = "matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_TemplateLanguage_And_DefaultLanguaged_NotSpecified_JMESPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = "to_array(@)[?heartrate]",
                    TypeMatchExpressionLanguage = ExpressionLanguage.JMESPath,
                    DeviceIdExpression = "device",
                    DeviceIdExpressionLanguage = ExpressionLanguage.JMESPath,
                    TimestampExpression = "date",
                    TimestampExpressionLanguage = ExpressionLanguage.JMESPath,
                    CorrelationIdExpression = "session",
                    CorrelationIdExpressionLanguage = ExpressionLanguage.JMESPath,
                    PatientIdExpression = "patient",
                    PatientIdExpressionLanguage = ExpressionLanguage.JMESPath,
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "heartrate", Required = false,  ValueExpressionLanguage = ExpressionLanguage.JMESPath },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = "matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_TemplateLanguage_And_DefaultLanguaged_Specified_JMESPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = "to_array(@)[?heartrate]",
                    TypeMatchExpressionLanguage = ExpressionLanguage.JMESPath,
                    DeviceIdExpression = "device",
                    DeviceIdExpressionLanguage = ExpressionLanguage.JMESPath,
                    TimestampExpression = "date",
                    TimestampExpressionLanguage = ExpressionLanguage.JMESPath,
                    CorrelationIdExpression = "session",
                    CorrelationIdExpressionLanguage = ExpressionLanguage.JMESPath,
                    PatientIdExpression = "patient",
                    PatientIdExpressionLanguage = ExpressionLanguage.JMESPath,
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "heartrate", Required = false,  ValueExpressionLanguage = ExpressionLanguage.JMESPath },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = "matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_MissingRequiredValueExpression_And_UsingJsonPath_ExceptionIsThrown_Test()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = "$..[?(@heartrate)]",
                    DeviceIdExpression = "$.device",
                    TimestampExpression = "$.date",
                    CorrelationIdExpression = "$.session",
                    PatientIdExpression = "$.patient",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.missingField", Required = true },
                    },
                });
            });
        }

        [Fact]
        public void Given_MissingRequiredValueExpression_And_UsingJMESPath_ExceptionIsThrown_Test()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = "to_array(@)[?heartrate]",
                    DeviceIdExpression = "device",
                    TimestampExpression = "date",
                    CorrelationIdExpression = "session",
                    PatientIdExpression = "patient",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "missingField", Required = true },
                    },
                });
            });
        }

        private void PerformEvaluation(IContentTemplate contentTemplate)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new
            {
                heartrate = "60",
                device = "abc",
                date = time,
                session = "abcdefg",
                patient = "patient123",
            });

            var result = contentTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("abc", m.DeviceId);
                Assert.Equal("patient123", m.PatientId);
                Assert.Equal("abcdefg", m.CorrelationId);
                Assert.Null(m.EncounterId);
                Assert.Collection(
                    m.Properties,
                    p =>
                    {
                        Assert.Equal("hr", p.Name);
                        Assert.Equal("60", p.Value);
                    },
                    p =>
                    {
                        Assert.Equal("pie", p.Name);
                        Assert.Equal("patient123", p.Value);
                    });
            });
        }
    }
}
