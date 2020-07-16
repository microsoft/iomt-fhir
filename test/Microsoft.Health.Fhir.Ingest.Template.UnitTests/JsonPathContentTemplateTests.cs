// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class JsonPathContentTemplateTests
    {
        private static readonly IContentTemplate SingleValueTemplate = new JsonPathContentTemplate
        {
            TypeName = "heartrate",
            TypeMatchExpression = "$..[?(@heartrate)]",
            DeviceIdExpression = "$.device",
            TimestampExpression = "$.date",
            Values = new List<JsonPathValueExpression>
            {
              new JsonPathValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
            },
        };

        private static readonly IContentTemplate SingleValueOptionalContentTemplate = new JsonPathContentTemplate
        {
            TypeName = "heartrate",
            TypeMatchExpression = "$..[?(@heartrate)]",
            DeviceIdExpression = "$.device",
            TimestampExpression = "$.date",
            PatientIdExpression = "$.pid",
            EncounterIdExpression = "$.eid",
            Values = new List<JsonPathValueExpression>
            {
              new JsonPathValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
            },
        };

        private static readonly IContentTemplate SingleValueRequiredTemplate = new JsonPathContentTemplate
        {
            TypeName = "heartrate",
            TypeMatchExpression = "$..[?(@heartrate)]",
            DeviceIdExpression = "$.device",
            TimestampExpression = "$.date",
            Values = new List<JsonPathValueExpression>
            {
              new JsonPathValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = true },
            },
        };

        private static readonly IContentTemplate MultiValueTemplate = new JsonPathContentTemplate
        {
            TypeName = "hrStepCombo",
            TypeMatchExpression = "$..[?(@heartrate || @steps)]",
            DeviceIdExpression = "$.device",
            TimestampExpression = "$.date",
            Values = new List<JsonPathValueExpression>
            {
              new JsonPathValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
              new JsonPathValueExpression { ValueName = "steps", ValueExpression = "$.steps", Required = false },
            },
        };

        private static readonly IContentTemplate MultiValueRequiredTemplate = new JsonPathContentTemplate
        {
            TypeName = "bloodpressure",
            TypeMatchExpression = "$..[?(@systolic)]",
            DeviceIdExpression = "$.device",
            TimestampExpression = "$.date",
            Values = new List<JsonPathValueExpression>
            {
              new JsonPathValueExpression { ValueName = "systolic", ValueExpression = "$.systolic", Required = true },
              new JsonPathValueExpression { ValueName = "diastolic", ValueExpression = "$.diastolic", Required = true },
            },
        };

        private static readonly IContentTemplate SingleValueRequiredCompoundAndMatchTemplate = new JsonPathContentTemplate
        {
            TypeName = "heartrate",
            TypeMatchExpression = "$..[?(@heartrate && @date)]",
            DeviceIdExpression = "$.device",
            TimestampExpression = "$.date",
            Values = new List<JsonPathValueExpression>
            {
              new JsonPathValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = true },
            },
        };

        private static readonly IContentTemplate CorrelationIdTemplate = new JsonPathContentTemplate
        {
            TypeName = "heartrate",
            TypeMatchExpression = "$..[?(@heartrate)]",
            DeviceIdExpression = "$.device",
            TimestampExpression = "$.date",
            CorrelationIdExpression = "$.session",
            Values = new List<JsonPathValueExpression>
            {
              new JsonPathValueExpression { ValueName = "hr", ValueExpression = "$.heartrate", Required = false },
            },
        };

        [Fact]
        public void GivenMultiValueTemplateAndValidTokenWithMissingValue_WhenGetMeasurements_ThenSingleMeasurementReturned_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time });

            var result = MultiValueTemplate.GetMeasurements(token).ToArray();

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

        [Fact]
        public void GivenMultiValueTemplateAndValidTokenWithAllValues_WhenGetMeasurements_ThenSingleMeasurementReturned_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", steps ="2", device = "abc", date = time });

            var result = MultiValueTemplate.GetMeasurements(token).ToArray();

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

        [Fact]
        public void GivenMultiValueTemplateAndValidTokenArrayWithAllValues_WhenGetMeasurements_ThenSingleMeasurementReturned_Test()
        {
            var time = DateTime.UtcNow;

            var token = JToken.FromObject(new[]
            {
                new { systolic = "120", diastolic = "80", device = "abc", date = time },
                new { systolic = "122", diastolic = "82", device = "abc", date = time.AddMinutes(-1) },
                new { systolic = "100", diastolic = "70", device = "abc", date = time.AddMinutes(-2) },
            });

            var result = MultiValueRequiredTemplate.GetMeasurements(token).ToArray();

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

        [Fact]
        public void GivenMultiValueRequiredTemplateAndValidTokenWithMissingValue_WhenGetMeasurements_ThenInvalidOperationException_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { systolic = "120", device = "abc", date = time });

            Assert.Throws<InvalidOperationException>(() => MultiValueRequiredTemplate.GetMeasurements(token).ToArray());
        }

        [Fact]
        public void GivenMultiValueRequiredTemplateAndValidTokenWithAllValues_WhenGetMeasurements_ThenSingleMeasurementReturned_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { systolic = "120", diastolic = "80", device = "abc", date = time });

            var result = MultiValueRequiredTemplate.GetMeasurements(token).ToArray();

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

        [Fact]
        public void GivenSingleValueTemplateAndValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time });

            var result = SingleValueTemplate.GetMeasurements(token).ToArray();

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
        public void GivenSingleValueOptionalContentTemplateAndValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time, pid = "123", eid = "789" });

            var result = SingleValueOptionalContentTemplate.GetMeasurements(token).ToArray();

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

        [Fact]
        public void GivenSingleValueCompoundAndTemplateAndValidToken_WhenGetMeasurements_ThenSingleMeasurementReturned_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time });

            var result = SingleValueRequiredCompoundAndMatchTemplate.GetMeasurements(token).ToArray();

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

            result = SingleValueRequiredTemplate.GetMeasurements(token).ToArray();
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

        [Fact]
        public void GivenSingleValueCompoundAndTemplateAndPartialToken_WhenGetMeasurements_ThenEmptyIEnumerableReturned_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", mdate = time });

            var result = SingleValueRequiredCompoundAndMatchTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GivenTemplateAndInvalidToken_WhenGetMeasurements_ThenEmptyIEnumerableReturned_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { foo = "60", device = "abc", date = time });

            var result = SingleValueTemplate.GetMeasurements(token).ToArray();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GivenTemplateWithCorrelationIdAndIdPresent_WhenGetMeasurements_ThenCorrelationIdReturn_Test()
        {
            var time = DateTime.UtcNow;
            var session = Guid.NewGuid().ToString();
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time, session });

            var result = CorrelationIdTemplate.GetMeasurements(token).ToArray();

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

        [Fact]
        public void GivenTemplateWithCorrelationIdAndIdMissing_WhenGetMeasurements_ThenArgumentNullExceptionThrown_Test()
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new { heartrate = "60", device = "abc", date = time });

            var ex = Assert.Throws<ArgumentNullException>(() => CorrelationIdTemplate.GetMeasurements(token).ToArray());
            Assert.Contains("correlationId", ex.Message);
        }

        public class JsonWidget
        {
            [JsonProperty(PropertyName = "My Property")]
            public string MyProperty { get; set; }

            public DateTime Time { get; set; } = DateTime.UtcNow;
        }
    }
}
