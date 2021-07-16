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
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    CorrelationIdExpression = new Expression("$.session"),
                    PatientIdExpression = new Expression("$.patient"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "pie", Value = "$.matchedToken.patient", Required = false },
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
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    CorrelationIdExpression = new Expression("$.session"),
                    PatientIdExpression = new Expression("$.patient"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "pie", Value = "$.matchedToken.patient", Required = false },
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
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]", ExpressionLanguage.JsonPath),
                    DeviceIdExpression = new Expression("$.device", ExpressionLanguage.JsonPath),
                    TimestampExpression = new Expression("$.date", ExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new Expression("$.session", ExpressionLanguage.JsonPath),
                    PatientIdExpression = new Expression("$.patient", ExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false,  Language = ExpressionLanguage.JsonPath },
                        new CalculatedFunctionValueExpression { ValueName = "pie", Value = "$.matchedToken.patient", Required = false },
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
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]", ExpressionLanguage.JsonPath),
                    DeviceIdExpression = new Expression("$.device", ExpressionLanguage.JsonPath),
                    TimestampExpression = new Expression("$.date", ExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new Expression("$.session", ExpressionLanguage.JsonPath),
                    PatientIdExpression = new Expression("$.patient", ExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false,  Language = ExpressionLanguage.JsonPath },
                        new CalculatedFunctionValueExpression { ValueName = "pie", Value = "$.matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_NoTemplateLanguage_And_DefaultLanguaged_Specified_JMESPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = ExpressionLanguage.JmesPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate]"),
                    DeviceIdExpression = new Expression("device"),
                    TimestampExpression = new Expression("date"),
                    CorrelationIdExpression = new Expression("session"),
                    PatientIdExpression = new Expression("patient"),

                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "pie", Value = "matchedToken.patient", Required = false },
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
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate]", ExpressionLanguage.JmesPath),
                    DeviceIdExpression = new Expression("device", ExpressionLanguage.JmesPath),
                    TimestampExpression = new Expression("date", ExpressionLanguage.JmesPath),
                    CorrelationIdExpression = new Expression("session", ExpressionLanguage.JmesPath),
                    PatientIdExpression = new Expression("patient", ExpressionLanguage.JmesPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "heartrate", Required = false,  Language = ExpressionLanguage.JmesPath },
                        new CalculatedFunctionValueExpression { ValueName = "pie", Value = "matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_TemplateLanguage_And_DefaultLanguaged_Specified_JMESPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = ExpressionLanguage.JmesPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate]", ExpressionLanguage.JmesPath),
                    DeviceIdExpression = new Expression("device", ExpressionLanguage.JmesPath),
                    TimestampExpression = new Expression("date", ExpressionLanguage.JmesPath),
                    CorrelationIdExpression = new Expression("session", ExpressionLanguage.JmesPath),
                    PatientIdExpression = new Expression("patient", ExpressionLanguage.JmesPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "heartrate", Required = false,  Language = ExpressionLanguage.JmesPath },
                        new CalculatedFunctionValueExpression { ValueName = "pie", Value = "matchedToken.patient", Required = false },
                    },
                });
        }

        [Fact]
        public void Given_MissingRequiredValueExpression_And_UsingJsonPath_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<InvalidOperationException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    CorrelationIdExpression = new Expression("$.session"),
                    PatientIdExpression = new Expression("$.patient"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.missingField", Required = true },
                    },
                });
            });
            Assert.StartsWith("Unable to extract required value for [hr]", exp.Message);
        }

        [Fact]
        public void Given_MissingRequiredValueExpression_And_UsingJMESPath_ExceptionIsThrown_Test()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = ExpressionLanguage.JmesPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate]", ExpressionLanguage.JmesPath),
                    DeviceIdExpression = new Expression("device", ExpressionLanguage.JmesPath),
                    TimestampExpression = new Expression("date", ExpressionLanguage.JmesPath),
                    CorrelationIdExpression = new Expression("session", ExpressionLanguage.JmesPath),
                    PatientIdExpression = new Expression("patient", ExpressionLanguage.JmesPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "missingField", Required = true },
                    },
                });
            });
        }

        [Fact]
        public void Given_MissingRequiredDeviceIdExpression_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<InvalidOperationException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    TimestampExpression = new Expression("$.date"),
                    CorrelationIdExpression = new Expression("$.session"),
                    PatientIdExpression = new Expression("$.patient"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.missingField", Required = true },
                    },
                });
            });
            Assert.StartsWith("An expression must be set for [DeviceIdExpression]", exp.Message);
        }

        [Fact]
        public void Given_MissingRequiredTimestampExpression_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<InvalidOperationException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    CorrelationIdExpression = new Expression("$.session"),
                    PatientIdExpression = new Expression("$.patient"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.missingField", Required = true },
                    },
                });
            });
            Assert.StartsWith("An expression must be set for [TimestampExpression]", exp.Message);
        }

        [Fact]
        public void Given_MissingRequiredCorrelationIdValue_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<InvalidOperationException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    CorrelationIdExpression = new Expression("$.missingsession"),
                    PatientIdExpression = new Expression("$.patient"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.missingField", Required = true },
                    },
                });
            });
            Assert.StartsWith("Unable to extract required value for [CorrelationIdExpression]", exp.Message);
        }

        [Fact]
        public void Given_MissingRequiredDeviceIdValue_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<InvalidOperationException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.badDeviceExpression"),
                    TimestampExpression = new Expression("$.date"),
                    CorrelationIdExpression = new Expression("$.session"),
                    PatientIdExpression = new Expression("$.patient"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.missingField", Required = true },
                    },
                });
            });
            Assert.StartsWith("Unable to extract required value for [DeviceIdExpression]", exp.Message);
        }

        private void PerformEvaluation(CalculatedFunctionContentTemplate template)
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

            var measurementExtractor = new MeasurementExtractor(template, new ExpressionEvaluatorFactory(template.DefaultExpressionLanguage));
            var result = measurementExtractor.GetMeasurements(token).ToArray();

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
