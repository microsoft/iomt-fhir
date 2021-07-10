// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CalculatedFunctionContentTemplateTests
    {
        [Theory]
        [MemberData(nameof(GetMultiValueTemplates))]
        public void GivenMultiValueTemplateAndValidTokenWithMissingValue_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time });

            var result = template.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("hrStepCombo", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("abc", m.DeviceId);
                Assert.Null(m.PatientId);
                Assert.Null(m.EncounterId);
                Assert.Null(m.CorrelationId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("60", p.Value);
                });
            });
        }

        [Theory]
        [MemberData(nameof(GetMultiValueTemplates))]
        public void GivenMultiValueTemplateAndValidTokenWithAllValues_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", steps ="2", device = "abc", date = time });

            var result = template.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("hrStepCombo", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("abc", m.DeviceId);
                Assert.Null(m.PatientId);
                Assert.Null(m.EncounterId);
                Assert.Null(m.CorrelationId);
                Assert.Collection(
                    m.Properties,
                    p =>
                    {
                        Assert.Equal("hr", p.Name);
                        Assert.Equal("60", p.Value);
                    },
                    p =>
                    {
                        Assert.Equal("steps", p.Name);
                        Assert.Equal("2", p.Value);
                    });
            });
        }

        [Theory]
        [MemberData(nameof(GetMultiValueRequiredTemplates))]
        public void GivenMultiValueTemplateAndValidTokenArrayWithAllValues_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;

            var token = JToken.FromObject(
                new
                {
                    Body = new[]
                    {
                        new { systolic = "120", diastolic = "80", device = "abc", date = time },
                        new { systolic = "122", diastolic = "82", device = "abc", date = time.AddMinutes(-1) },
                        new { systolic = "100", diastolic = "70", device = "abc", date = time.AddMinutes(-2) },
                    },
                });

            var result = template.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(
                result,
                m =>
                {
                    Assert.Equal("bloodpressure", m.Type);
                    Assert.Equal(time, m.OccurrenceTimeUtc);
                    Assert.Equal("abc", m.DeviceId);
                    Assert.Null(m.PatientId);
                    Assert.Null(m.EncounterId);
                    Assert.Null(m.CorrelationId);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("systolic", p.Name);
                            Assert.Equal("120", p.Value);
                        },
                        p =>
                        {
                            Assert.Equal("diastolic", p.Name);
                            Assert.Equal("80", p.Value);
                        });
                },
                m =>
                {
                    Assert.Equal("bloodpressure", m.Type);
                    Assert.Equal(time.AddMinutes(-1), m.OccurrenceTimeUtc);
                    Assert.Equal("abc", m.DeviceId);
                    Assert.Null(m.PatientId);
                    Assert.Null(m.EncounterId);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("systolic", p.Name);
                            Assert.Equal("122", p.Value);
                        },
                        p =>
                        {
                            Assert.Equal("diastolic", p.Name);
                            Assert.Equal("82", p.Value);
                        });
                },
                m =>
                {
                    Assert.Equal("bloodpressure", m.Type);
                    Assert.Equal(time.AddMinutes(-2), m.OccurrenceTimeUtc);
                    Assert.Equal("abc", m.DeviceId);
                    Assert.Null(m.PatientId);
                    Assert.Null(m.EncounterId);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("systolic", p.Name);
                            Assert.Equal("100", p.Value);
                        },
                        p =>
                        {
                            Assert.Equal("diastolic", p.Name);
                            Assert.Equal("70", p.Value);
                        });
                });
        }

        [Theory]
        [MemberData(nameof(GetMultiValueRequiredTemplates))]
        public void GivenMultiValueRequiredTemplateAndValidTokenWithMissingValue_WhenGetMeasurements_ThenInvalidOperationException_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(
                new
                {
                    Body = new[]
                    {
                        new { systolic = "120", device = "abc", date = time },
                    },
                });

            Assert.Throws<InvalidOperationException>(() => template.GetMeasurements(token).ToArray());
        }

        [Theory]
        [MemberData(nameof(GetMultiValueRequiredTemplates))]
        public void GivenMultiValueRequiredTemplateAndValidTokenWithAllValues_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(
                new
                {
                    Body = new[]
                    {
                        new { systolic = "120", diastolic = "80", device = "abc", date = time },
                    },
                });

            var result = template.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("bloodpressure", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("abc", m.DeviceId);
                Assert.Null(m.PatientId);
                Assert.Null(m.EncounterId);
                Assert.Null(m.CorrelationId);
                Assert.Collection(
                    m.Properties,
                    p =>
                    {
                        Assert.Equal("systolic", p.Name);
                        Assert.Equal("120", p.Value);
                    },
                    p =>
                    {
                        Assert.Equal("diastolic", p.Name);
                        Assert.Equal("80", p.Value);
                    });
            });
        }

        [Theory]
        [MemberData(nameof(GetSingleValueTemplates))]
        public void GivenSingleValueTemplateAndValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(IContentTemplate contentTemplate)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time });

            var result = contentTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("abc", m.DeviceId);
                Assert.Null(m.PatientId);
                Assert.Null(m.EncounterId);
                Assert.Null(m.CorrelationId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("60", p.Value);
                });
            });
        }

        [Theory]
        [MemberData(nameof(GetSingleValueOptionalContentTemplates))]
        public void GivenSingleValueOptionalContentTemplateAndValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(IContentTemplate contentTemplate)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time, pid = "123", eid = "789" });

            var result = contentTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("abc", m.DeviceId);
                Assert.Equal("123", m.PatientId);
                Assert.Equal("789", m.EncounterId);
                Assert.Null(m.CorrelationId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("60", p.Value);
                });
            });
        }

        [Theory]
        [MemberData(nameof(GetSingleValueRequiredCompoundAndMatchTemplates))]
        public void GivenSingleValueCompoundAndTemplateAndValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time });

            var result = template.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("abc", m.DeviceId);
                Assert.Null(m.PatientId);
                Assert.Null(m.EncounterId);
                Assert.Null(m.CorrelationId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("60", p.Value);
                });
            });
        }

        [Theory]
        [MemberData(nameof(GetSingleValueRequiredTemplates))]
        public void GivenSingleValueRequiredTemplateAndValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time });

            var result = template.GetMeasurements(token).ToArray();
            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("abc", m.DeviceId);
                Assert.Null(m.PatientId);
                Assert.Null(m.EncounterId);
                Assert.Null(m.CorrelationId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("60", p.Value);
                });
            });
        }

        [Fact]
        public void GivenPropertyWithSpace_WhenGetMeasurements_ThenSingleMeasurementReturned_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new JsonWidget { MyProperty = "data", Time = time });

            var template = new CalculatedFunctionContentTemplate
            {
                TypeName = "space",
                TypeMatchExpression = new Expression("$..[?(@['My Property'])]"),
                DeviceIdExpression = new Expression("$.['My Property']"),
                TimestampExpression = new Expression("$.Time"),
                Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "prop", Value = "$.['My Property']", Required = false },
                    },
                ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(),
            };

            var result = template.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("space", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("data", m.DeviceId);
                Assert.Null(m.PatientId);
                Assert.Null(m.EncounterId);
                Assert.Null(m.CorrelationId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("prop", p.Name);
                    Assert.Equal("data", p.Value);
                });
            });
        }

        [Theory]
        [MemberData(nameof(GetSingleValueRequiredCompoundAndMatchTemplates))]
        public void GivenSingleValueCompoundAndTemplateAndPartialToken_WhenGetMeasurements_ThenEmptyIEnumerableReturned_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", mdate = time });

            var result = template.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(GetSingleValueTemplates))]
        public void GivenTemplateAndInvalidToken_WhenGetMeasurements_ThenEmptyIEnumerableReturned_Test(IContentTemplate contentTemplate)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { foo = "60", device = "abc", date = time });

            var result = contentTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(GetCorrelationIdTemplates))]
        public void GivenTemplateWithCorrelationIdAndIdPresent_WhenGetMeasurements_ThenCorrelationIdReturn_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;
            var session = Guid.NewGuid().ToString();
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time, session });

            var result = template.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(result, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal(time, m.OccurrenceTimeUtc);
                Assert.Equal("abc", m.DeviceId);
                Assert.Null(m.PatientId);
                Assert.Null(m.EncounterId);
                Assert.Equal(session, m.CorrelationId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("60", p.Value);
                });
            });
        }

        [Theory]
        [MemberData(nameof(GetCorrelationIdTemplates))]
        public void GivenTemplateWithCorrelationIdAndIdMissing_WhenGetMeasurements_ThenArgumentNullExceptionThrown_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time });

            var ex = Assert.Throws<InvalidOperationException>(() => template.GetMeasurements(token).ToArray());
            Assert.Contains("Unable to extract required value for [CorrelationIdExpression]", ex.Message);
        }

        [Theory]
        [MemberData(nameof(GetParentScopeAndMultiValueRequiredTemplates))]
        public void GivenParentScopeAndMultiValueTemplateAndValidTokenArrayWithAllValues_WhenGetMeasurements_ThenSingleMeasurementReturned_Test(IContentTemplate template)
        {
            var time = DateTime.UtcNow;

            var token = JToken.FromObject(
                new
                {
                    Properties = new { deviceId = "parentScopedDeviceId" },
                    Body = new[]
                    {
                        new { systolic = "120", diastolic = "80", date = time },
                        new { systolic = "122", diastolic = "82", date = time.AddMinutes(-1) },
                    },
                });

            var result = template.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Collection(
                result,
                m =>
                {
                    Assert.Equal("bloodpressure", m.Type);
                    Assert.Equal(time, m.OccurrenceTimeUtc);
                    Assert.Equal("parentScopedDeviceId", m.DeviceId);
                    Assert.Null(m.PatientId);
                    Assert.Null(m.EncounterId);
                    Assert.Null(m.CorrelationId);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("systolic", p.Name);
                            Assert.Equal("120", p.Value);
                        },
                        p =>
                        {
                            Assert.Equal("diastolic", p.Name);
                            Assert.Equal("80", p.Value);
                        });
                },
                m =>
                {
                    Assert.Equal("bloodpressure", m.Type);
                    Assert.Equal(time.AddMinutes(-1), m.OccurrenceTimeUtc);
                    Assert.Equal("parentScopedDeviceId", m.DeviceId);
                    Assert.Null(m.PatientId);
                    Assert.Null(m.EncounterId);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("systolic", p.Name);
                            Assert.Equal("122", p.Value);
                        },
                        p =>
                        {
                            Assert.Equal("diastolic", p.Name);
                            Assert.Equal("82", p.Value);
                        });
                });
        }

        public static IEnumerable<object[]> GetSingleValueTemplates()
        {
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate]"),
                    DeviceIdExpression = new Expression("device"),
                    TimestampExpression = new Expression("date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "heartrate", Required = false },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(ExpressionLanguage.JmesPath),
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(),
                },
            }.ToArray();
        }

        public static IEnumerable<object[]> GetSingleValueOptionalContentTemplates()
        {
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    PatientIdExpression = new Expression("$.pid"),
                    EncounterIdExpression = new Expression("$.eid"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(),
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate]"),
                    DeviceIdExpression = new Expression("device"),
                    TimestampExpression = new Expression("date"),
                    PatientIdExpression = new Expression("pid"),
                    EncounterIdExpression = new Expression("eid"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "heartrate", Required = false },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(ExpressionLanguage.JmesPath),
                },
            }.ToArray();
        }

        public static IEnumerable<object[]> GetMultiValueTemplates()
        {
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "hrStepCombo",
                    TypeMatchExpression = new Expression("$..[?(@heartrate || @steps)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                      new CalculatedFunctionValueExpression { ValueName = "steps", Value = "$.steps", Required = false },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(),
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "hrStepCombo",
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate || steps]"),
                    DeviceIdExpression = new Expression("device"),
                    TimestampExpression = new Expression("date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "steps", Value = "steps", Required = false },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(ExpressionLanguage.JmesPath),
                },
            }.ToArray();
        }

        public static IEnumerable<object[]> GetMultiValueRequiredTemplates()
        {
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "bloodpressure",
                    TypeMatchExpression = new Expression("$..[?(@systolic)]"),
                    DeviceIdExpression = new Expression("$.matchedToken.device"),
                    TimestampExpression = new Expression("$.matchedToken.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "systolic", Value = "$.matchedToken.systolic", Required = true },
                      new CalculatedFunctionValueExpression { ValueName = "diastolic", Value = "$.matchedToken.diastolic", Required = true },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(),
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "bloodpressure",
                    TypeMatchExpression = new Expression("Body[?systolic]"),
                    DeviceIdExpression = new Expression("matchedToken.device"),
                    TimestampExpression = new Expression("matchedToken.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "systolic", Value = "matchedToken.systolic", Required = true },
                        new CalculatedFunctionValueExpression { ValueName = "diastolic", Value = "matchedToken.diastolic", Required = true },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(ExpressionLanguage.JmesPath),
                },
            }.ToArray();
        }

        public static IEnumerable<object[]> GetSingleValueRequiredTemplates()
        {
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = true },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(),
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate]"),
                    DeviceIdExpression = new Expression("matchedToken.device"),
                    TimestampExpression = new Expression("matchedToken.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "matchedToken.heartrate", Required = true },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(ExpressionLanguage.JmesPath),
                },
            }.ToArray();
        }

        public static IEnumerable<object[]> GetSingleValueRequiredCompoundAndMatchTemplates()
        {
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate && @date)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = true },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(),
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    DefaultExpressionLanguage = ExpressionLanguage.JmesPath,
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate && date]"),
                    DeviceIdExpression = new Expression("matchedToken.device"),
                    TimestampExpression = new Expression("matchedToken.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "matchedToken.heartrate", Required = true },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(ExpressionLanguage.JmesPath),
                },
            }.ToArray();
        }

        public static IEnumerable<object[]> GetCorrelationIdTemplates()
        {
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = new Expression("$..[?(@heartrate)]"),
                    DeviceIdExpression = new Expression("$.device"),
                    TimestampExpression = new Expression("$.date"),
                    CorrelationIdExpression = new Expression("$.session"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", Value = "$.heartrate", Required = false },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(),
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    DefaultExpressionLanguage = ExpressionLanguage.JmesPath,
                    TypeMatchExpression = new Expression("to_array(@)[?heartrate]"),
                    DeviceIdExpression = new Expression("matchedToken.device"),
                    TimestampExpression = new Expression("matchedToken.date"),
                    CorrelationIdExpression = new Expression("matchedToken.session"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", Value = "matchedToken.heartrate", Required = false },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(ExpressionLanguage.JmesPath),
                },
            }.ToArray();
        }

        public static IEnumerable<object[]> GetParentScopeAndMultiValueRequiredTemplates()
        {
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "bloodpressure",
                    TypeMatchExpression = new Expression("$..[?(@systolic)]"),
                    DeviceIdExpression = new Expression("$.Properties.deviceId"),
                    TimestampExpression = new Expression("$.matchedToken.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "systolic", Value = "$.matchedToken.systolic", Required = true },
                      new CalculatedFunctionValueExpression { ValueName = "diastolic", Value = "$.matchedToken.diastolic", Required = true },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(),
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "bloodpressure",
                    DefaultExpressionLanguage = ExpressionLanguage.JmesPath,
                    TypeMatchExpression = new Expression("Body[?systolic]"),
                    DeviceIdExpression = new Expression("Properties.deviceId"),
                    TimestampExpression = new Expression("matchedToken.date"),
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "systolic", Value = "matchedToken.systolic", Required = true },
                        new CalculatedFunctionValueExpression { ValueName = "diastolic", Value = "matchedToken.diastolic", Required = true },
                    },
                    ExpressionEvaluatorFactory = new ExpressionEvaluatorFactory(ExpressionLanguage.JmesPath),
                },
            }.ToArray();
        }

        public class JsonWidget
        {
            [JsonProperty(PropertyName = "My Property")]
            public string MyProperty { get; set; }

            public DateTime Time { get; set; } = DateTime.UtcNow;
        }
    }
}
