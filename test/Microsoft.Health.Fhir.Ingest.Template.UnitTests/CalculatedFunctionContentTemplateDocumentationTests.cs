// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using DevLab.JmesPath;
using Microsoft.Health.Expressions;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CalculatedFunctionContentTemplateDocumentationTests
    {
        private CollectionContentTemplateFactory _collectionContentTemplateFactory;

        public CalculatedFunctionContentTemplateDocumentationTests()
        {
            var logger = Substitute.For<ITelemetryLogger>();

            var jmesPath = new JmesPath();
            var functionRegister = new AssemblyExpressionRegister(typeof(IExpressionRegister).Assembly, logger);
            functionRegister.RegisterExpressions(jmesPath.FunctionRepository);

            _collectionContentTemplateFactory = new CollectionContentTemplateFactory(
                new JsonPathContentTemplateFactory(),
                new IotJsonPathContentTemplateFactory(),
                new IotCentralJsonPathContentTemplateFactory(),
                new CalculatedFunctionContentTemplateFactory(new TemplateExpressionEvaluatorFactory(jmesPath), logger));
        }

        [Fact]
        public void MultipleMatches_And_UsingParentScope_Test()
        {
            var template = CreateTemplate("TestInput/data_Documentation_CalculatedFunction_MultipleMatches_Template.json");
            var data = JObject.Parse(LoadJson("TestInput/data_Documentation_CalculatedFunction_MultipleMatches_Payload.json"));
            var measurements = template.GetMeasurements(data).ToArray();

            Assert.NotEmpty(measurements);
            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2021-07-13T17:29:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
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
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2021-07-13T17:28:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
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

        [Fact]
        public void Extract_HeartRate_Test()
        {
            var template = CreateTemplate("TestInput/data_Documentation_CalculatedFunction_HeartRate_Template.json");
            var data = JObject.Parse(LoadJson("TestInput/data_Documentation_CalculatedFunction_HeartRate_Payload.json"));
            var measurements = template.GetMeasurements(data).ToArray();

            Assert.NotEmpty(measurements);
            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2019-02-01T22:46:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("hr", p.Name);
                            Assert.Equal("78", p.Value);
                        });
                });
        }

        [Fact]
        public void Extract_BloodPressure_Test()
        {
            var template = CreateTemplate("TestInput/data_Documentation_CalculatedFunction_BloodPressure_Template.json");
            var data = JObject.Parse(LoadJson("TestInput/data_Documentation_CalculatedFunction_BloodPressure_Payload.json"));
            var measurements = template.GetMeasurements(data).ToArray();

            Assert.NotEmpty(measurements);
            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2019-02-01T22:46:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("systolic", p.Name);
                            Assert.Equal("123", p.Value);
                        },
                        p =>
                        {
                            Assert.Equal("diastolic", p.Name);
                            Assert.Equal("87", p.Value);
                        });
                });
        }

        [Fact]
        public void Extract_MultipleMeasurements_Test()
        {
            var template = CreateTemplate("TestInput/data_Documentation_CalculatedFunction_MultipleMeasurements_Template.json");
            var data = JObject.Parse(LoadJson("TestInput/data_Documentation_CalculatedFunction_MultipleMeasurements_Payload.json"));
            var measurements = template.GetMeasurements(data).ToArray();

            Assert.NotEmpty(measurements);
            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2019-02-01T22:46:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("hr", p.Name);
                            Assert.Equal("78", p.Value);
                        });
                },
                m =>
                {
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2019-02-01T22:46:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("steps", p.Name);
                            Assert.Equal("2", p.Value);
                        });
                });
        }

        [Fact]
        public void Extract_MultipleMeasurements_FromArray_Test()
        {
            var template = CreateTemplate("TestInput/data_Documentation_CalculatedFunction_HeartRate_Template.json");
            var data = JObject.Parse(LoadJson("TestInput/data_Documentation_CalculatedFunction_MultipleMeasurements_FromArray_Payload.json"));
            var measurements = template.GetMeasurements(data).ToArray();

            Assert.NotEmpty(measurements);
            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2019-02-01T20:46:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("hr", p.Name);
                            Assert.Equal("78", p.Value);
                        });
                },
                m =>
                {
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2019-02-01T21:46:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("hr", p.Name);
                            Assert.Equal("81", p.Value);
                        });
                },
                m =>
                {
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2019-02-01T22:46:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("hr", p.Name);
                            Assert.Equal("72", p.Value);
                        });
                });
        }

        [Fact]
        public void Extract_TransformData_Test()
        {
            var template = CreateTemplate("TestInput/data_Documentation_CalculatedFunction_TransformData_Template.json");
            var data = JObject.Parse(LoadJson("TestInput/data_Documentation_CalculatedFunction_TransformData_Payload.json"));
            var measurements = template.GetMeasurements(data).ToArray();

            Assert.NotEmpty(measurements);
            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("heightInMeters", m.Type);
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2019-02-01T22:46:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("height", p.Name);
                            Assert.Equal(1.9812, float.Parse(p.Value), 4);
                        });
                },
                m =>
                {
                    Assert.Equal("heightInMeters", m.Type);
                    Assert.Equal("device123", m.DeviceId);
                    Assert.Equal("2019-02-01T23:46:01", m.OccurrenceTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("height", p.Name);
                            Assert.Equal("1.9304", p.Value);
                        });
                });
        }

        private IContentTemplate CreateTemplate(string templateFilePath)
        {
            var data = LoadJson(templateFilePath);
            return _collectionContentTemplateFactory.Create(data).Template;
        }

        private string LoadJson(string filePath)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            return File.ReadAllText(path);
        }
    }
}
