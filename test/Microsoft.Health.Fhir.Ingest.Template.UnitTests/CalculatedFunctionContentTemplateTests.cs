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

            var template = new JsonPathContentTemplate
            {
                TypeName = "space",
                TypeMatchExpression = "$..[?(@['My Property'])]",
                DeviceIdExpression = "$.['My Property']",
                TimestampExpression = "$.Time",
                Values = new List<JsonPathValueExpression>
                {
                    new JsonPathValueExpression { ValueName = "prop", ValueExpression = "$.['My Property']", Required = false },
                },
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

            var ex = Assert.Throws<ArgumentNullException>(() => template.GetMeasurements(token).ToArray());
            Assert.Contains("correlationId", ex.Message);
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
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeMatchExpression = "to_array(@)[?heartrate]",
                    DeviceIdExpression = "device",
                    TimestampExpression = "date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "heartrate", Required = false },
                    },
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    TypeMatchExpression = "$..[?(@heartrate)]",
                    DeviceIdExpression = "$.device",
                    TimestampExpression = "$.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
                    },
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
                    TypeMatchExpression = "$..[?(@heartrate)]",
                    DeviceIdExpression = "$.device",
                    TimestampExpression = "$.date",
                    PatientIdExpression = "$.pid",
                    EncounterIdExpression = "$.eid",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
                    },
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeMatchExpression = "to_array(@)[?heartrate]",
                    DeviceIdExpression = "device",
                    TimestampExpression = "date",
                    PatientIdExpression = "pid",
                    EncounterIdExpression = "eid",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "heartrate", Required = false },
                    },
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
                    TypeMatchExpression = "$..[?(@heartrate || @steps)]",
                    DeviceIdExpression = "$.device",
                    TimestampExpression = "$.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
                      new CalculatedFunctionValueExpression { ValueName = "steps", ValueExpression = "$.steps", Required = false },
                    },
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "hrStepCombo",
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeMatchExpression = "to_array(@)[?heartrate || steps]",
                    DeviceIdExpression = "device",
                    TimestampExpression = "date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "heartrate", Required = false },
                        new CalculatedFunctionValueExpression { ValueName = "steps", ValueExpression = "steps", Required = false },
                    },
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
                    TypeMatchExpression = "$..[?(@systolic)]",
                    DeviceIdExpression = "$.matchedToken.device",
                    TimestampExpression = "$.matchedToken.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "systolic", ValueExpression = "$.matchedToken.systolic", Required = true },
                      new CalculatedFunctionValueExpression { ValueName = "diastolic", ValueExpression = "$.matchedToken.diastolic", Required = true },
                    },
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "bloodpressure",
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeMatchExpression = "Body[?systolic]",
                    DeviceIdExpression = "matchedToken.device",
                    TimestampExpression = "matchedToken.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "systolic", ValueExpression = "matchedToken.systolic", Required = true },
                        new CalculatedFunctionValueExpression { ValueName = "diastolic", ValueExpression = "matchedToken.diastolic", Required = true },
                    },
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
                    TypeMatchExpression = "$..[?(@heartrate)]",
                    DeviceIdExpression = "$.device",
                    TimestampExpression = "$.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = true },
                    },
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeMatchExpression = "to_array(@)[?heartrate]",
                    DeviceIdExpression = "matchedToken.device",
                    TimestampExpression = "matchedToken.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "matchedToken.heartrate", Required = true },
                    },
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
                    TypeMatchExpression = "$..[?(@heartrate && @date)]",
                    DeviceIdExpression = "$.device",
                    TimestampExpression = "$.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = true },
                    },
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeMatchExpression = "to_array(@)[?heartrate && date]",
                    DeviceIdExpression = "matchedToken.device",
                    TimestampExpression = "matchedToken.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "matchedToken.heartrate", Required = true },
                    },
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
                    TypeMatchExpression = "$..[?(@heartrate)]",
                    DeviceIdExpression = "$.device",
                    TimestampExpression = "$.date",
                    CorrelationIdExpression = "$.session",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
                    },
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "heartrate",
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeMatchExpression = "to_array(@)[?heartrate]",
                    DeviceIdExpression = "matchedToken.device",
                    TimestampExpression = "matchedToken.date",
                    CorrelationIdExpression = "matchedToken.session",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "hr", ValueExpression = "matchedToken.heartrate", Required = false },
                    },
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
                    TypeMatchExpression = "$..[?(@systolic)]",
                    DeviceIdExpression = "$.Properties.deviceId",
                    TimestampExpression = "$.matchedToken.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression { ValueName = "systolic", ValueExpression = "$.matchedToken.systolic", Required = true },
                      new CalculatedFunctionValueExpression { ValueName = "diastolic", ValueExpression = "$.matchedToken.diastolic", Required = true },
                    },
                },
            }.ToArray();
            yield return new List<IContentTemplate>()
            {
                new CalculatedFunctionContentTemplate
                {
                    TypeName = "bloodpressure",
                    DefaultExpressionLanguage = ExpressionLanguage.JMESPath,
                    TypeMatchExpression = "Body[?systolic]",
                    DeviceIdExpression = "Properties.deviceId",
                    TimestampExpression = "matchedToken.date",
                    Values = new List<CalculatedFunctionValueExpression>
                    {
                        new CalculatedFunctionValueExpression { ValueName = "systolic", ValueExpression = "matchedToken.systolic", Required = true },
                        new CalculatedFunctionValueExpression { ValueName = "diastolic", ValueExpression = "matchedToken.diastolic", Required = true },
                    },
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
