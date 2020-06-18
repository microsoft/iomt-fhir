// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class TimePeriodMeasurementObservationGroupFactoryTests
    {
        [Fact]
        public void GivenSingleBoundaryWithSingleMeasurementSingleValue_WhenBuild_SingleObservationGroupWithSingleValueReturned_Test()
        {
            var seedDate = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var measurement = new IMeasurement[]
            {
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate,
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "1" },
                    },
                },
            };

            var measureGroup = Substitute.For<IMeasurementGroup>()
                .Mock(mg => mg.MeasureType.Returns("a"))
                .Mock(mg => mg.Data.Returns(measurement));

            var factory = new TimePeriodMeasurementObservationGroupFactory(ObservationPeriodInterval.Single);

            var result = factory.Build(measureGroup)?.ToArray();
            Assert.NotNull(result);
            Assert.Single(result);

            Assert.Collection(
                result,
                og =>
                {
                    Assert.Equal(seedDate, og.Boundary.Start);
                    Assert.Equal(seedDate, og.Boundary.End);

                    var properties = og.GetValues().ToArray();
                    Assert.Single(properties);
                    Assert.Collection(
                        properties,
                        p =>
                        {
                            Assert.Equal("a", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate, v.Time);
                                Assert.Equal("1", v.Value);
                            });
                        });
                });
        }

        [Fact]
        public void GivenSingleBoundaryWithMultipleMeasurementMultipleValue_WhenBuild_MultipleObservationGroupWithMultipleValueReturned_Test()
        {
            var seedDate = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var measurement = new IMeasurement[]
            {
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate,
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "1" },
                        new MeasurementProperty { Name = "b", Value = "2" },
                    },
                },
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate.AddMinutes(1),
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "3" },
                        new MeasurementProperty { Name = "b", Value = "4" },
                    },
                },
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate.AddDays(1),
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "5" },
                        new MeasurementProperty { Name = "b", Value = "6" },
                    },
                },
            };

            var measureGroup = Substitute.For<IMeasurementGroup>()
                .Mock(mg => mg.MeasureType.Returns("a"))
                .Mock(mg => mg.Data.Returns(measurement));

            var factory = new TimePeriodMeasurementObservationGroupFactory(ObservationPeriodInterval.Single);

            var result = factory.Build(measureGroup)?.ToArray();
            Assert.NotNull(result);
            Assert.Equal(3, result.Length);

            Assert.Collection(
                result,
                og =>
                {
                    Assert.Equal(seedDate, og.Boundary.Start);
                    Assert.Equal(seedDate, og.Boundary.End);

                    var properties = og.GetValues().ToArray();
                    Assert.Equal(2, properties.Length);
                    Assert.Collection(
                        properties,
                        p =>
                        {
                            Assert.Equal("a", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate, v.Time);
                                Assert.Equal("1", v.Value);
                            });
                        },
                        p =>
                        {
                            Assert.Equal("b", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate, v.Time);
                                Assert.Equal("2", v.Value);
                            });
                        });
                },
                og =>
                {
                    Assert.Equal(seedDate.AddMinutes(1), og.Boundary.Start);
                    Assert.Equal(seedDate.AddMinutes(1), og.Boundary.End);

                    var properties = og.GetValues().ToArray();
                    Assert.Equal(2, properties.Length);
                    Assert.Collection(
                        properties,
                        p =>
                        {
                            Assert.Equal("a", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate.AddMinutes(1), v.Time);
                                Assert.Equal("3", v.Value);
                            });
                        },
                        p =>
                        {
                            Assert.Equal("b", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate.AddMinutes(1), v.Time);
                                Assert.Equal("4", v.Value);
                            });
                        });
                },
                og =>
                {
                    Assert.Equal(seedDate.AddDays(1), og.Boundary.Start);
                    Assert.Equal(seedDate.AddDays(1), og.Boundary.End);

                    var properties = og.GetValues().ToArray();
                    Assert.Equal(2, properties.Length);
                    Assert.Collection(
                        properties,
                        p =>
                        {
                            Assert.Equal("a", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate.AddDays(1), v.Time);
                                Assert.Equal("5", v.Value);
                            });
                        },
                        p =>
                        {
                            Assert.Equal("b", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate.AddDays(1), v.Time);
                                Assert.Equal("6", v.Value);
                            });
                        });
                });
        }

        [Fact]
        public void GivenHourlyBoundaryWithMultipleMeasurementMultipleValue_WhenBuild_MultipleObservationGroupWithMultipleValueReturned_Test()
        {
            var seedDate = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var measurement = new IMeasurement[]
            {
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate,
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "1" },
                        new MeasurementProperty { Name = "b", Value = "2" },
                    },
                },
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate.AddHours(1),
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "3" },
                        new MeasurementProperty { Name = "b", Value = "4" },
                    },
                },
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate.AddMinutes(1),
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "5" },
                        new MeasurementProperty { Name = "b", Value = "6" },
                    },
                },
            };

            var measureGroup = Substitute.For<IMeasurementGroup>()
                .Mock(mg => mg.MeasureType.Returns("a"))
                .Mock(mg => mg.Data.Returns(measurement));

            var factory = new TimePeriodMeasurementObservationGroupFactory(ObservationPeriodInterval.Hourly);

            var result = factory.Build(measureGroup)?.ToArray();
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);

            Assert.Collection(
                result,
                og =>
                {
                    Assert.Equal(seedDate, og.Boundary.Start);
                    Assert.Equal(seedDate.AddHours(1).AddTicks(-1), og.Boundary.End);

                    var properties = og.GetValues().ToArray();
                    Assert.Equal(2, properties.Length);
                    Assert.Collection(
                        properties,
                        p =>
                        {
                            Assert.Equal("a", p.Key);
                            Assert.Equal(2, p.Value.Count());
                            Assert.Collection(
                                p.Value,
                                v =>
                                {
                                    Assert.Equal(seedDate, v.Time);
                                    Assert.Equal("1", v.Value);
                                },
                                v =>
                                {
                                    Assert.Equal(seedDate.AddMinutes(1), v.Time);
                                    Assert.Equal("5", v.Value);
                                });
                        },
                        p =>
                        {
                            Assert.Equal("b", p.Key);
                            Assert.Equal(2, p.Value.Count());
                            Assert.Collection(
                                p.Value,
                                v =>
                                {
                                    Assert.Equal(seedDate, v.Time);
                                    Assert.Equal("2", v.Value);
                                },
                                v =>
                                {
                                    Assert.Equal(seedDate.AddMinutes(1), v.Time);
                                    Assert.Equal("6", v.Value);
                                });
                        });
                },
                og =>
                {
                    Assert.Equal(seedDate.AddHours(1), og.Boundary.Start);
                    Assert.Equal(seedDate.AddHours(2).AddTicks(-1), og.Boundary.End);

                    var properties = og.GetValues().ToArray();
                    Assert.Equal(2, properties.Length);
                    Assert.Collection(
                        properties,
                        p =>
                        {
                            Assert.Equal("a", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate.AddHours(1), v.Time);
                                Assert.Equal("3", v.Value);
                            });
                        },
                        p =>
                        {
                            Assert.Equal("b", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate.AddHours(1), v.Time);
                                Assert.Equal("4", v.Value);
                            });
                        });
                });
        }

        [Fact]
        public void GivenDailyBoundaryWithMultipleMeasurementMultipleValue_WhenBuild_MultipleObservationGroupWithMultipleValueReturned_Test()
        {
            var seedDate = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var measurement = new IMeasurement[]
            {
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate,
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "1" },
                        new MeasurementProperty { Name = "b", Value = "2" },
                    },
                },
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate.AddDays(1),
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "3" },
                        new MeasurementProperty { Name = "b", Value = "4" },
                    },
                },
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate.AddMinutes(1),
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "5" },
                        new MeasurementProperty { Name = "b", Value = "6" },
                    },
                },
            };

            var measureGroup = Substitute.For<IMeasurementGroup>()
                .Mock(mg => mg.MeasureType.Returns("a"))
                .Mock(mg => mg.Data.Returns(measurement));

            var factory = new TimePeriodMeasurementObservationGroupFactory(ObservationPeriodInterval.Daily);

            var result = factory.Build(measureGroup)?.ToArray();
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);

            Assert.Collection(
                result,
                og =>
                {
                    Assert.Equal(seedDate, og.Boundary.Start);
                    Assert.Equal(seedDate.AddDays(1).AddTicks(-1), og.Boundary.End);

                    var properties = og.GetValues().ToArray();
                    Assert.Equal(2, properties.Length);
                    Assert.Collection(
                        properties,
                        p =>
                        {
                            Assert.Equal("a", p.Key);
                            Assert.Equal(2, p.Value.Count());
                            Assert.Collection(
                                p.Value,
                                v =>
                                {
                                    Assert.Equal(seedDate, v.Time);
                                    Assert.Equal("1", v.Value);
                                },
                                v =>
                                {
                                    Assert.Equal(seedDate.AddMinutes(1), v.Time);
                                    Assert.Equal("5", v.Value);
                                });
                        },
                        p =>
                        {
                            Assert.Equal("b", p.Key);
                            Assert.Equal(2, p.Value.Count());
                            Assert.Collection(
                                p.Value,
                                v =>
                                {
                                    Assert.Equal(seedDate, v.Time);
                                    Assert.Equal("2", v.Value);
                                },
                                v =>
                                {
                                    Assert.Equal(seedDate.AddMinutes(1), v.Time);
                                    Assert.Equal("6", v.Value);
                                });
                        });
                },
                og =>
                {
                    Assert.Equal(seedDate.AddDays(1), og.Boundary.Start);
                    Assert.Equal(seedDate.AddDays(2).AddTicks(-1), og.Boundary.End);

                    var properties = og.GetValues().ToArray();
                    Assert.Equal(2, properties.Length);
                    Assert.Collection(
                        properties,
                        p =>
                        {
                            Assert.Equal("a", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate.AddDays(1), v.Time);
                                Assert.Equal("3", v.Value);
                            });
                        },
                        p =>
                        {
                            Assert.Equal("b", p.Key);
                            Assert.Single(p.Value);
                            Assert.Collection(p.Value, v =>
                            {
                                Assert.Equal(seedDate.AddDays(1), v.Time);
                                Assert.Equal("4", v.Value);
                            });
                        });
                });
        }
    }
}