// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeableConceptFhirValueProcessorTests
    {
        [Fact]
        public void GivenValidTemplate_WhenCreateValue_ThenSampledDataProperlyConfigured_Test()
        {
            var processor = new CodeableConceptFhirValueProcessor();
            var template = new CodeableConceptFhirValueType
            {
                Text = "myText",
                Codes = new List<FhirCode>
                {
                    new FhirCode { Code = "c1", Display = "d1", System = "s1" },
                    new FhirCode { Code = "c2", Display = "d2", System = "s2" },
                },
            };

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "value") }));

            var result = processor.CreateValue(template, data) as CodeableConcept;
            Assert.NotNull(result);
            Assert.Equal("myText", result.Text);
            Assert.Collection(
                result.Coding,
                c =>
                {
                    Assert.Equal("c1", c.Code);
                    Assert.Equal("d1", c.Display);
                    Assert.Equal("s1", c.System);
                },
                c =>
                {
                    Assert.Equal("c2", c.Code);
                    Assert.Equal("d2", c.Display);
                    Assert.Equal("s2", c.System);
                });
        }

        [Fact]
        public void GivenInvalidDataTypeType_WhenMergeValue_ThenNotSupportedExceptionThrown_Test()
        {
            var processor = new CodeableConceptFhirValueProcessor();
            var template = new CodeableConceptFhirValueType();

            Assert.Throws<NotSupportedException>(() => processor.MergeValue(template, default, new FhirDateTime()));
        }

        [Fact]
        public void GivenValidTemplate_WhenMergeValue_ThenMergeValueReturned_Test()
        {
            var processor = new CodeableConceptFhirValueProcessor();
            var template = new CodeableConceptFhirValueType
            {
                Text = "myText",
                Codes = new List<FhirCode>
                {
                    new FhirCode { Code = "c1", Display = "d1", System = "s1" },
                    new FhirCode { Code = "c2", Display = "d2", System = "s2" },
                },
            };

            var oldValue = new CodeableConcept
            {
                Text = "Test",
                Coding = new List<Coding>
                {
                    new Coding { Code = "old" },
                },
            };

            var data = Substitute.For<IObservationData>()
                .Mock(m => m.DataPeriod.Returns((DateTime.UtcNow, DateTime.UtcNow)))
                .Mock(m => m.Data.Returns(new (DateTime, string)[] { (DateTime.UtcNow, "value") }));

            var result = processor.MergeValue(template, data, oldValue) as CodeableConcept;
            Assert.NotNull(result);
            Assert.Equal("myText", result.Text);
            Assert.Collection(
                result.Coding,
                c =>
                {
                    Assert.Equal("c1", c.Code);
                    Assert.Equal("d1", c.Display);
                    Assert.Equal("s1", c.System);
                },
                c =>
                {
                    Assert.Equal("c2", c.Code);
                    Assert.Equal("d2", c.Display);
                    Assert.Equal("s2", c.System);
                });
        }
    }
}
