// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class CorrelationMeasurementObservationGroupTests
    {
        [Fact]
        public void GivenSingleMeasurement_WhenGetBoundary_ThenBoundarySet_Test()
        {
            var start = DateTime.UtcNow.AddDays(-1);

            var og = new CorrelationMeasurementObservationGroup("test");

            og.AddMeasurement(new Measurement { OccurrenceTimeUtc = start });

            Assert.Equal(start, og.Boundary.Start);
            Assert.Equal(start, og.Boundary.End);
        }

        [Fact]
        public void GivenMultipleMeasurement_WhenGetBoundary_ThenBoundaryMatchesMinAndMax_Test()
        {
            var start = DateTime.UtcNow.AddDays(-1);
            var end = DateTime.UtcNow;

            var og = new CorrelationMeasurementObservationGroup("test");

            og.AddMeasurement(new Measurement { OccurrenceTimeUtc = end });
            og.AddMeasurement(new Measurement { OccurrenceTimeUtc = start.AddHours(2) });
            og.AddMeasurement(new Measurement { OccurrenceTimeUtc = start });
            og.AddMeasurement(new Measurement { OccurrenceTimeUtc = start.AddHours(1) });

            Assert.Equal(start, og.Boundary.Start);
            Assert.Equal(end, og.Boundary.End);
        }

        [Fact]
        public void GivenValidObservationGroup_WhenGetIdSegment_CorrectValueReturned_Test()
        {
            var id = Guid.NewGuid().ToString();
            var og = new CorrelationMeasurementObservationGroup(id);

            var segment = og.GetIdSegment();
            Assert.Equal(id, segment);
        }

        [Fact]
        public void GivenDifferentNamedValues_WhenAddMeasurement_ThenGetValuesReturnsSorted_Test()
        {
            var startDate = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);

            var measurement = new IMeasurement[]
               {
                    new Measurement
                    {
                        OccurrenceTimeUtc = startDate,
                        Properties = new List<MeasurementProperty>
                        {
                            new MeasurementProperty { Name = "a", Value = "1" },
                        },
                    },
                    new Measurement
                    {
                        OccurrenceTimeUtc = endDate,
                        Properties = new List<MeasurementProperty>
                        {
                            new MeasurementProperty { Name = "a", Value = "2" },
                        },
                    },
                    new Measurement
                    {
                        OccurrenceTimeUtc = startDate.AddHours(1),
                        Properties = new List<MeasurementProperty>
                        {
                            new MeasurementProperty { Name = "a", Value = "3" },
                        },
                    },
                    new Measurement
                    {
                        OccurrenceTimeUtc = endDate,
                        Properties = new List<MeasurementProperty>
                        {
                            new MeasurementProperty { Name = "b", Value = "1" },
                        },
                    },
                    new Measurement
                    {
                        OccurrenceTimeUtc = startDate,
                        Properties = new List<MeasurementProperty>
                        {
                            new MeasurementProperty { Name = "b", Value = "2" },
                        },
                    },
                    new Measurement
                    {
                        OccurrenceTimeUtc = startDate,
                        Properties = new List<MeasurementProperty>
                        {
                            new MeasurementProperty { Name = "c", Value = "3" },
                        },
                    },
               };

            var og = new CorrelationMeasurementObservationGroup("test");

            foreach (var m in measurement)
            {
                og.AddMeasurement(m);
            }

            var values = og.GetValues();

            var aValues = values["a"];

            Assert.Collection(
                aValues,
                v =>
                {
                    Assert.Equal(startDate, v.Time);
                    Assert.Equal("1", v.Value);
                },
                v =>
                {
                    Assert.Equal(measurement[2].OccurrenceTimeUtc, v.Time);
                    Assert.Equal("3", v.Value);
                },
                v =>
                {
                    Assert.Equal(endDate, v.Time);
                    Assert.Equal("2", v.Value);
                });

            var bValues = values["b"];

            Assert.Collection(
                bValues,
                v =>
                {
                    Assert.Equal(startDate, v.Time);
                    Assert.Equal("2", v.Value);
                },
                v =>
                {
                    Assert.Equal(endDate, v.Time);
                    Assert.Equal("1", v.Value);
                });

            var cValues = values["c"];

            Assert.Collection(
                cValues,
                v =>
                {
                    Assert.Equal(startDate, v.Time);
                    Assert.Equal("3", v.Value);
                });
        }
    }
}
