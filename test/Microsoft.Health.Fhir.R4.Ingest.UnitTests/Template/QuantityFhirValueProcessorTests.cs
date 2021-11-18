// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class QuantityFhirValueProcessorTests
    {
        [Fact]
        public void GivenValidTemplate_WhenCreateValue_ThenSampledDataProperlyConfigured_Test()
        {
            var processor = new QuantityFhirValueProcessor();
            var template = new QuantityFhirValueType
            {
                Unit = "myUnit",
                System = "mySystem",
                Code = "myCode",
            };

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "22.4") }));

            var result = processor.CreateValue(template, data) as Quantity;
            Assert.NotNull(result);
            Assert.Equal("myUnit", result.Unit);
            Assert.Equal("mySystem", result.System);
            Assert.Equal("myCode", result.Code);
            Assert.Equal(22.4m, result.Value);
        }

        [Fact]
        public void GivenInvalidDataValue_WhenCreateValue_ThenInvalidQuantityFhirValueExceptionThrown_Test()
        {
            var processor = new QuantityFhirValueProcessor();
            var template = new QuantityFhirValueType
            {
                Unit = "myUnit",
                System = "mySystem",
                Code = "myCode",
            };

            // invalid format for data value
            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "NaN") }));

            Assert.Throws<InvalidQuantityFhirValueException>(() => processor.CreateValue(template, data));

            // multiple data values
            data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "22.4"), (DateTime.UtcNow, "22.4") }));

            Assert.Throws<InvalidQuantityFhirValueException>(() => processor.CreateValue(template, data));

            // no data value
            data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { }));

            Assert.Throws<InvalidQuantityFhirValueException>(() => processor.CreateValue(template, data));
        }

        [Fact]
        public void GivenInvalidDataTypeType_WhenMergeValue_ThenNotSupportedExceptionThrown_Test()
        {
            var processor = new QuantityFhirValueProcessor();
            var template = new QuantityFhirValueType();

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "value") }));

            Assert.Throws<NotSupportedException>(() => processor.MergeValue(template, data, new FhirDateTime()));
        }

        [Fact]
        public void GivenValidTemplate_WhenMergeValue_ThenMergeValueReturned_Test()
        {
            var processor = new QuantityFhirValueProcessor();
            var template = new QuantityFhirValueType
            {
                Unit = "myUnit",
                System = "mySystem",
                Code = "myCode",
            };

            var oldQuantity = new Quantity { Value = 1, System = "s", Code = "c", Unit = "u" };

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "22.4") }));

            var result = processor.MergeValue(template, data, oldQuantity) as Quantity;
            Assert.NotNull(result);
            Assert.Equal("myUnit", result.Unit);
            Assert.Equal("mySystem", result.System);
            Assert.Equal("myCode", result.Code);
            Assert.Equal(22.4m, result.Value);
        }
    }
}
