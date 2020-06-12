// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class CorrelationMeasurementObservationGroupFactoryTests
    {
        [Fact]
        public void GivenMultipleMeasurementsWithCorrelationId_WhenBuild_AllMeasurementsReturnInSingleObservationGroupSortedByDate_Test()
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
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate.AddDays(1),
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "2" },
                    },
                },
                new Measurement
                {
                    OccurrenceTimeUtc = seedDate.AddHours(1),
                    Properties = new List<MeasurementProperty>
                    {
                        new MeasurementProperty { Name = "a", Value = "3" },
                    },
                },
            };

            var measureGroup = Substitute.For<IMeasurementGroup>()
                .Mock(mg => mg.MeasureType.Returns("a"))
                .Mock(mg => mg.CorrelationId.Returns("id"))
                .Mock(mg => mg.Data.Returns(measurement));

            var factory = new CorrelationMeasurementObservationGroupFactory();

            var result = factory.Build(measureGroup)?.ToArray();
            Assert.NotNull(result);
            Assert.Single(result);

            Assert.Collection(
                result,
                og =>
                {
                    Assert.Equal(seedDate, og.Boundary.Start);
                    Assert.Equal(measurement[1].OccurrenceTimeUtc, og.Boundary.End);

                    Assert.Equal("a", og.Name);

                    var properties = og.GetValues().ToArray();
                    Assert.Single(properties);

                    Assert.Collection(
                        properties,
                        p =>
                        {
                            Assert.Equal("a", p.Key);
                            Assert.Collection(
                                p.Value,
                                v =>
                                {
                                    Assert.Equal(seedDate, v.Time);
                                    Assert.Equal("1", v.Value);
                                },
                                v =>
                                {
                                    Assert.Equal(measurement[2].OccurrenceTimeUtc, v.Time);
                                    Assert.Equal("3", v.Value);
                                },
                                v =>
                                {
                                    Assert.Equal(measurement[1].OccurrenceTimeUtc, v.Time);
                                    Assert.Equal("2", v.Value);
                                });
                        });
                });
        }

        [Fact]
        public void GivenMeasurementGroupWithoutCorrelationId_WhenBuild_ThenCorrelationIdNotDefinedExceptionThrown_Test()
        {
            var measureGroup = Substitute.For<IMeasurementGroup>()
                .Mock(mg => mg.CorrelationId.Returns((string)null));

            var factory = new CorrelationMeasurementObservationGroupFactory();

            Assert.Throws<CorrelationIdNotDefinedException>(() => factory.Build(measureGroup)?.ToArray());
        }
    }
}
