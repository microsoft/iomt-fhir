// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Hl7.Fhir.Model;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class StringFhirValueProcessorTests
    {
        [Fact]
        public void GivenValidTemplate_WhenCreateValue_ThenStringProperlyConfigured_Test()
        {
            var processor = new StringFhirValueProcessor();
            var template = new StringFhirValueType() { };

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "my string value") }));

            var result = processor.CreateValue(template, data) as FhirString;
            Assert.NotNull(result);
            Assert.Equal("my string value", result.Value);
        }

        [Fact]
        public void GivenInvalidDataTypeType_WhenMergeValue_ThenNotSupportedExceptionThrown_Test()
        {
            var processor = new StringFhirValueProcessor();
            var template = new StringFhirValueType();

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "a string") }));

            Assert.Throws<NotSupportedException>(() => processor.MergeValue(template, data, new FhirDateTime()));
        }

        [Fact]
        public void GivenValidTemplate_WhenMergeValue_ThenMergeValueReturned_Test()
        {
            var processor = new StringFhirValueProcessor();
            var template = new StringFhirValueType();

            FhirString oldString = new FhirString("old string");

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "new string") }));

            var result = processor.MergeValue(template, data, oldString) as FhirString;
            Assert.NotNull(result);
            Assert.Equal("new string", result.Value);
        }
    }
}
