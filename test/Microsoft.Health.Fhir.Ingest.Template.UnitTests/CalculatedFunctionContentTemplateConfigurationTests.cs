// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
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
                    TypeMatchExpression = new TemplateExpression("$..[?(@heartrate)]", TemplateExpressionLanguage.JsonPath),
                    DeviceIdExpression = new TemplateExpression("$.device", TemplateExpressionLanguage.JsonPath),
                    TimestampExpression = new TemplateExpression("$.date", TemplateExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new TemplateExpression("$.session", TemplateExpressionLanguage.JsonPath),
                    PatientIdExpression = new TemplateExpression("$.patient", TemplateExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.heartrate", TemplateExpressionLanguage.JsonPath), Required = false, },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = new TemplateExpression("$.matchedToken.patient", TemplateExpressionLanguage.JsonPath), Required = false, },
                    },
                });
        }

        [Fact]
        public void Given_NoTemplateLanguage_And_DefaultLanguaged_Specified_JsonPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = TemplateExpressionLanguage.JsonPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = new TemplateExpression("$..[?(@heartrate)]", TemplateExpressionLanguage.JsonPath),
                    DeviceIdExpression = new TemplateExpression("$.device", TemplateExpressionLanguage.JsonPath),
                    TimestampExpression = new TemplateExpression("$.date", TemplateExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new TemplateExpression("$.session", TemplateExpressionLanguage.JsonPath),
                    PatientIdExpression = new TemplateExpression("$.patient", TemplateExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.heartrate", TemplateExpressionLanguage.JsonPath), Required = false, },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = new TemplateExpression("$.matchedToken.patient", TemplateExpressionLanguage.JsonPath), Required = false, },
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
                    TypeMatchExpression = new TemplateExpression("$..[?(@heartrate)]", TemplateExpressionLanguage.JsonPath),
                    DeviceIdExpression = new TemplateExpression("$.device", TemplateExpressionLanguage.JsonPath),
                    TimestampExpression = new TemplateExpression("$.date", TemplateExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new TemplateExpression("$.session", TemplateExpressionLanguage.JsonPath),
                    PatientIdExpression = new TemplateExpression("$.patient", TemplateExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.heartrate", TemplateExpressionLanguage.JsonPath), Required = false, },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = new TemplateExpression("$.matchedToken.patient", TemplateExpressionLanguage.JsonPath), Required = false, },
                    },
                });
        }

        [Fact]
        public void Given_TemplateLanguage_And_DefaultLanguaged_Specified_JsonPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = TemplateExpressionLanguage.JsonPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = new TemplateExpression("$..[?(@heartrate)]", TemplateExpressionLanguage.JsonPath),
                    DeviceIdExpression = new TemplateExpression("$.device", TemplateExpressionLanguage.JsonPath),
                    TimestampExpression = new TemplateExpression("$.date", TemplateExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new TemplateExpression("$.session", TemplateExpressionLanguage.JsonPath),
                    PatientIdExpression = new TemplateExpression("$.patient", TemplateExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.heartrate", TemplateExpressionLanguage.JsonPath), Required = false, },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = new TemplateExpression("$.matchedToken.patient", TemplateExpressionLanguage.JsonPath), Required = false, },
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
                    TypeMatchExpression = new TemplateExpression("to_array(@)[?heartrate]", TemplateExpressionLanguage.JmesPath),
                    DeviceIdExpression = new TemplateExpression("device", TemplateExpressionLanguage.JmesPath),
                    TimestampExpression = new TemplateExpression("date", TemplateExpressionLanguage.JmesPath),
                    CorrelationIdExpression = new TemplateExpression("session", TemplateExpressionLanguage.JmesPath),
                    PatientIdExpression = new TemplateExpression("patient", TemplateExpressionLanguage.JmesPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("matchedToken.heartrate", TemplateExpressionLanguage.JmesPath), Required = false, },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = new TemplateExpression("matchedToken.patient", TemplateExpressionLanguage.JmesPath), Required = false, },
                    },
                });
        }

        [Fact]
        public void Given_TemplateLanguage_And_DefaultLanguaged_Specified_JMESPathIsUsed_Test()
        {
            PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = TemplateExpressionLanguage.JmesPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = new TemplateExpression("to_array(@)[?heartrate]", TemplateExpressionLanguage.JmesPath),
                    DeviceIdExpression = new TemplateExpression("device", TemplateExpressionLanguage.JmesPath),
                    TimestampExpression = new TemplateExpression("date", TemplateExpressionLanguage.JmesPath),
                    CorrelationIdExpression = new TemplateExpression("session", TemplateExpressionLanguage.JmesPath),
                    PatientIdExpression = new TemplateExpression("patient", TemplateExpressionLanguage.JmesPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("matchedToken.heartrate", TemplateExpressionLanguage.JmesPath), Required = false, },
                        new CalculatedFunctionValueExpression { ValueName = "pie", ValueExpression = new TemplateExpression("matchedToken.patient", TemplateExpressionLanguage.JmesPath), Required = false, },
                    },
                });
        }

        [Fact]
        public void Given_MissingRequiredValueExpression_And_UsingJsonPath_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<IncompatibleDataException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new TemplateExpression("$..[?(@heartrate)]", TemplateExpressionLanguage.JsonPath),
                    DeviceIdExpression = new TemplateExpression("$.device", TemplateExpressionLanguage.JsonPath),
                    TimestampExpression = new TemplateExpression("$.date", TemplateExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new TemplateExpression("$.session", TemplateExpressionLanguage.JsonPath),
                    PatientIdExpression = new TemplateExpression("$.patient", TemplateExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.heartrate", TemplateExpressionLanguage.JsonPath), Required = false, },
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.missingField", TemplateExpressionLanguage.JsonPath), Required = true, },
                    },
                });
            });
            Assert.StartsWith("Unable to extract required value for [hr]", exp.Message);
        }

        [Fact]
        public void Given_MissingRequiredValueExpression_And_UsingJMESPath_ExceptionIsThrown_Test()
        {
            Assert.Throws<IncompatibleDataException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    DefaultExpressionLanguage = TemplateExpressionLanguage.JmesPath,
                    TypeName = "heartrate",
                    TypeMatchExpression = new TemplateExpression("to_array(@)[?heartrate]", TemplateExpressionLanguage.JmesPath),
                    DeviceIdExpression = new TemplateExpression("device", TemplateExpressionLanguage.JmesPath),
                    TimestampExpression = new TemplateExpression("date", TemplateExpressionLanguage.JmesPath),
                    CorrelationIdExpression = new TemplateExpression("session", TemplateExpressionLanguage.JmesPath),
                    PatientIdExpression = new TemplateExpression("patient", TemplateExpressionLanguage.JmesPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("heartrate", TemplateExpressionLanguage.JsonPath), Required = false, },
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("missingField", TemplateExpressionLanguage.JsonPath), Required = true,  },
                    },
                });
            });
        }

        [Fact]
        public void Given_MissingRequiredDeviceIdExpression_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<IncompatibleDataException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new TemplateExpression("$..[?(@heartrate)]", TemplateExpressionLanguage.JsonPath),
                    TimestampExpression = new TemplateExpression("$.date", TemplateExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new TemplateExpression("$.session", TemplateExpressionLanguage.JsonPath),
                    PatientIdExpression = new TemplateExpression("$.patient", TemplateExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.heartrate", TemplateExpressionLanguage.JsonPath), Required = false, },
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.missingField", TemplateExpressionLanguage.JsonPath), Required = true, },
                    },
                });
            });
            Assert.StartsWith("An expression must be set for [DeviceIdExpression]", exp.Message);
        }

        [Fact]
        public void Given_MissingRequiredTimestampExpression_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<IncompatibleDataException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new TemplateExpression("$..[?(@heartrate)]", TemplateExpressionLanguage.JsonPath),
                    DeviceIdExpression = new TemplateExpression("$.device", TemplateExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new TemplateExpression("$.session", TemplateExpressionLanguage.JsonPath),
                    PatientIdExpression = new TemplateExpression("$.patient", TemplateExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.heartrate", TemplateExpressionLanguage.JsonPath), Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.missingField", TemplateExpressionLanguage.JsonPath), Required = true },
                    },
                });
            });
            Assert.StartsWith("An expression must be set for [TimestampExpression]", exp.Message);
        }

        [Fact]
        public void Given_MissingRequiredCorrelationIdValue_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<IncompatibleDataException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new TemplateExpression("$..[?(@heartrate)]", TemplateExpressionLanguage.JsonPath),
                    DeviceIdExpression = new TemplateExpression("$.device", TemplateExpressionLanguage.JsonPath),
                    TimestampExpression = new TemplateExpression("$.date", TemplateExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new TemplateExpression("$.missingsession", TemplateExpressionLanguage.JsonPath),
                    PatientIdExpression = new TemplateExpression("$.patient", TemplateExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.heartrate", TemplateExpressionLanguage.JsonPath), Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.missingField", TemplateExpressionLanguage.JsonPath), Required = true },
                    },
                });
            });
            Assert.StartsWith("Unable to extract required value for [CorrelationIdExpression]", exp.Message);
        }

        [Fact]
        public void Given_MissingRequiredDeviceIdValue_ExceptionIsThrown_Test()
        {
            var exp = Assert.Throws<IncompatibleDataException>(() =>
            {
                PerformEvaluation(
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new TemplateExpression("$..[?(@heartrate)]", TemplateExpressionLanguage.JsonPath),
                    DeviceIdExpression = new TemplateExpression("$.badDeviceExpression", TemplateExpressionLanguage.JsonPath),
                    TimestampExpression = new TemplateExpression("$.date", TemplateExpressionLanguage.JsonPath),
                    CorrelationIdExpression = new TemplateExpression("$.session", TemplateExpressionLanguage.JsonPath),
                    PatientIdExpression = new TemplateExpression("$.patient", TemplateExpressionLanguage.JsonPath),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.heartrate", TemplateExpressionLanguage.JsonPath), Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = new TemplateExpression("$.missingField", TemplateExpressionLanguage.JsonPath), Required = true },
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

            var measurementExtractor = new MeasurementExtractor(template, new TemplateExpressionEvaluatorFactory());
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
