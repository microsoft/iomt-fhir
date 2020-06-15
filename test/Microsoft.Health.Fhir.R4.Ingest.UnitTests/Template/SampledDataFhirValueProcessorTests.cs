// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class SampledDataFhirValueProcessorTests
    {
        [Fact]
        public void GivenValidTemplate_WhenCreateValue_ThenSampledDataProperlyConfigured_Test()
        {
            var sdp = Substitute.For<SampledDataProcessor>()
                .Mock(m => m.BuildSampledData(default, default, default, default).ReturnsForAnyArgs("data"));

            var processor = new SampledDataFhirValueProcessor(sdp);
            var template = new SampledDataFhirValueType
            {
                DefaultPeriod = 10,
                Unit = "myUnits",
            };

            var values = new (DateTime, string)[] { (new DateTime(2019, 1, 2), "value") };

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((new DateTime(2019, 1, 1), new DateTime(2019, 1, 3))))
                .Mock(m => m.Data.Returns(values));

            var result = processor.CreateValue(template, data) as SampledData;
            Assert.NotNull(result);
            Assert.Equal(template.DefaultPeriod, result.Period);
            Assert.Equal(template.Unit, result.Origin.Unit);
            Assert.Equal("data", result.Data);
            Assert.Equal(1, result.Dimensions);

            sdp.Received(1).BuildSampledData(
                Arg.Is<(DateTime, string)[]>(
                    v => v.Length == 1 && v.All(i => i.Item1 == values[0].Item1 && i.Item2 == values[0].Item2)),
                data.DataPeriod.start,
                data.DataPeriod.end,
                template.DefaultPeriod);
        }

        [Fact]
        public void GivenInvalidElementType_WhenMergeValue_ThenNotSupportedExceptionThrown_Test()
        {
            var sdp = Substitute.For<SampledDataProcessor>();

            var processor = new SampledDataFhirValueProcessor(sdp);
            var template = new SampledDataFhirValueType();

            var values = new (DateTime, string)[] { (new DateTime(2019, 1, 2), "value") };

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((new DateTime(2019, 1, 1), new DateTime(2019, 1, 3))))
                .Mock(m => m.Data.Returns(values));

            Assert.Throws<NotSupportedException>(() => processor.MergeValue(template, data, new FhirDateTime()));
        }

        [Fact]
        public void GivenValidTemplate_WhenMergeValue_ThenMergeValueReturned_Test()
        {
            var existingValues = new (DateTime Time, string Value)[] { };
            var mergeData = new (DateTime Time, string Value)[] { };

            var sdp = Substitute.For<SampledDataProcessor>()
                .Mock(m => m.SampledDataToTimeValues(default, default, default).ReturnsForAnyArgs(existingValues))
                .Mock(m => m.MergeData(default, default).ReturnsForAnyArgs(mergeData))
                .Mock(m => m.BuildSampledData(default, default, default, default).ReturnsForAnyArgs("merged"));

            var processor = new SampledDataFhirValueProcessor(sdp);
            var template = new SampledDataFhirValueType { DefaultPeriod = 100 };
            var existingSampledData = new SampledData { Data = "data" };

            var values = new (DateTime, string)[] { (new DateTime(2019, 1, 2), "value") };

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((new DateTime(2019, 1, 1), new DateTime(2019, 1, 3))))
                .Mock(m => m.Data.Returns(values));

            var result = processor.MergeValue(template, data, existingSampledData) as SampledData;
            Assert.NotNull(result);
            Assert.Equal("merged", result.Data);

            sdp.Received(1).SampledDataToTimeValues("data", data.DataPeriod.start, 100);
            sdp.Received(1).MergeData(
                existingValues,
                Arg.Is<(DateTime, string)[]>(
                    v => v.Length == 1 && v.All(i => i.Item1 == values[0].Item1 && i.Item2 == values[0].Item2)));
            sdp.Received(1).BuildSampledData(mergeData, data.DataPeriod.start, data.DataPeriod.end, 100);
        }

        [Fact]
        public void GivenSampledDataWithGreaterThanOneDimension_WhenMergeValue_ThenNotSupportedExceptionThrown_Test()
        {
            var sdp = Substitute.For<SampledDataProcessor>();

            var processor = new SampledDataFhirValueProcessor(sdp);
            var template = new SampledDataFhirValueType();

            var values = new (DateTime, string)[] { (new DateTime(2019, 1, 2), "value") };

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((new DateTime(2019, 1, 1), new DateTime(2019, 1, 3))))
                .Mock(m => m.Data.Returns(values));

            Assert.Throws<NotSupportedException>(() => processor.MergeValue(template, data, new SampledData { Dimensions = 2 }));
        }
    }
}
