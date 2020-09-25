// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class NormalizationTests
    {
        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndSteps.json")]
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

        private static IContentTemplate CreateTemplate(string json)
        {
            EnsureArg.IsNotNull(json, nameof(json));
            var templateContext = CollectionContentTemplateFactory.Default.Create(json);
            Assert.NotNull(templateContext);

            templateContext.EnsureValid();

            return templateContext.Template;
        }
    }
}
