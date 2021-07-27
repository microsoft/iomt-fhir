// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class NormalizationTests
    {
        private CollectionContentTemplateFactory _collectionContentTemplateFactory;

        public NormalizationTests()
        {
            var logger = Substitute.For<ITelemetryLogger>();
            _collectionContentTemplateFactory = new CollectionContentTemplateFactory(
                new JsonPathContentTemplateFactory(),
                new IotJsonPathContentTemplateFactory(),
                new IotCentralJsonPathContentTemplateFactory(),
                new CalculatedFunctionContentTemplateFactory(new TemplateExpressionEvaluatorFactory(), logger));
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndSteps.json")]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndStepsJmesPath.json")]
        public void GivenMeasurementWithHrv_WhenGetMeasurements_ThenNoMeasurementsReturned_Test(string json)
        {
            var template = CreateTemplate(json);

            var time = DateTime.UtcNow;
            var data = JToken.FromObject(new { hrv = "5", device = "abc", date = time });

            var measurements = template.GetMeasurements(data).ToArray();

            Assert.Empty(measurements);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndSteps.json")]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndStepsJmesPath.json")]
        public void GivenMeasurementWithHeartRateAndSteps_WhenGetMeasurements_ThenTwoMeasurementsReturned_Test(string json)
        {
            var template = CreateTemplate(json);

            var time = DateTime.UtcNow;
            var data = JToken.FromObject(new { heartrate = "60", steps = "2", device = "abc", date = time });

            var measurements = template.GetMeasurements(data).ToArray();

            Assert.NotEmpty(measurements);

            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("abc", m.DeviceId);
                    Assert.Equal(time, m.OccurrenceTimeUtc);
                    Assert.Equal("heartrate", m.Type);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("hr", p.Name);
                            Assert.Equal("60", p.Value);
                        });
                },
                m =>
                {
                    Assert.Equal("abc", m.DeviceId);
                    Assert.Equal(time, m.OccurrenceTimeUtc);
                    Assert.Equal("steps", m.Type);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("steps", p.Name);
                            Assert.Equal("2", p.Value);
                        });
                });
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndSteps.json")]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndStepsJmesPath.json")]
        public void GivenMeasurementWithHeartRate_WhenGetMeasurements_ThenOneMeasurementReturned_Test(string json)
        {
            var template = CreateTemplate(json);

            var time = DateTime.UtcNow;
            var data = JToken.FromObject(new { heartrate = "60", hrv = "5", device = "abc", date = time });

            var measurements = template.GetMeasurements(data).ToArray();

            Assert.NotEmpty(measurements);

            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("abc", m.DeviceId);
                    Assert.Equal(time, m.OccurrenceTimeUtc);
                    Assert.Equal("heartrate", m.Type);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("hr", p.Name);
                            Assert.Equal("60", p.Value);
                        });
                });
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndSteps.json")]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndStepsJmesPath.json")]
        public void GivenMeasurementWithSteps_WhenGetMeasurements_ThenOneMeasurementReturned_Test(string json)
        {
            var template = CreateTemplate(json);

            var time = DateTime.UtcNow;
            var data = JToken.FromObject(new { hrv = "5", steps = "2", device = "abc", date = time });

            var measurements = template.GetMeasurements(data).ToArray();

            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("abc", m.DeviceId);
                    Assert.Equal(time, m.OccurrenceTimeUtc);
                    Assert.Equal("steps", m.Type);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("steps", p.Name);
                            Assert.Equal("2", p.Value);
                        });
                });

            Assert.NotEmpty(measurements);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMultipleIotCentralJsonPath.json", @"TestInput/data_IotCentralPayloadExampleMultipleMessages.json")]
        public void GivenMultipleTemplatesAndMultipleMessages_WhenGetMeasurements_ThenAllMeasurementsReturned_Test(string json, string payload)
        {
            var template = CreateTemplate(json);

            var time = DateTime.UtcNow;
            var data = JToken.Parse(payload);

            var measurements = template.GetMeasurements(data).ToArray();

            // Verify values for one of each measurement type
            Assert.NotEmpty(measurements);
            Assert.Equal(6, measurements.Length);
            Assert.Equal(2, measurements.Count(m => string.Equals(m.Type, "heartrate")));
            Assert.Equal(2, measurements.Count(m => string.Equals(m.Type, "bloodpressure")));
            Assert.Equal(2, measurements.Count(m => string.Equals(m.Type, "respiratoryrate")));

            var heartrateMeasurement = measurements.First(m => string.Equals(m.Type, "heartrate"));
            Assert.Single(heartrateMeasurement.Properties);
            Assert.Equal("hr", heartrateMeasurement.Properties[0].Name);
            Assert.Equal("75", heartrateMeasurement.Properties[0].Value);

            var bpMeasurement = measurements.First(m => string.Equals(m.Type, "bloodpressure"));
            Assert.Equal(2, bpMeasurement.Properties.Count());
            Assert.Equal("systolic", bpMeasurement.Properties[0].Name);
            Assert.Equal("62", bpMeasurement.Properties[0].Value);
            Assert.Equal("diastolic", bpMeasurement.Properties[1].Name);
            Assert.Equal("30", bpMeasurement.Properties[1].Value);

            var respiratoryrateMeasurement = measurements.First(m => string.Equals(m.Type, "respiratoryrate"));
            Assert.Single(respiratoryrateMeasurement.Properties);
            Assert.Equal("respiratoryrate", respiratoryrateMeasurement.Properties[0].Name);
            Assert.Equal("15", respiratoryrateMeasurement.Properties[0].Value);
        }

        private IContentTemplate CreateTemplate(string json)
        {
            EnsureArg.IsNotNull(json, nameof(json));
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);

            templateContext.EnsureValid();

            return templateContext.Template;
        }
    }
}
