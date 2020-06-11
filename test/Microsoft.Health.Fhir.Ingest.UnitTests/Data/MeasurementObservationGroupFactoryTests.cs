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
    public class MeasurementObservationGroupFactoryTests
    {
        [Fact]
        public void GivenPeriodIntervalCorrelationId_WhenBuild_ThenCorrelationMeasurementObservationGroupsReturned_Test()
        {
            var factory = new MeasurementObservationGroupFactory(ObservationPeriodInterval.CorrelationId);

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
                .Mock(mg => mg.Data.Returns(measurement))
                .Mock(mg => mg.CorrelationId.Returns("id"));

            var observationGroup = factory.Build(measureGroup).First();

            Assert.IsType<CorrelationMeasurementObservationGroup>(observationGroup);
        }

        [Theory]
        [InlineData(ObservationPeriodInterval.Daily)]
        [InlineData(ObservationPeriodInterval.Hourly)]
        [InlineData(ObservationPeriodInterval.Single)]
        public void GivenOtherPeriodInterval_WhenBuild_ThenTimePeriodMeasurementObservationGroupsReturned_Test(ObservationPeriodInterval periodInterval)
        {
            var factory = new MeasurementObservationGroupFactory(periodInterval);

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

            var observationGroup = factory.Build(measureGroup).First();

            Assert.IsType<TimePeriodMeasurementObservationGroup>(observationGroup);
        }
    }
}
