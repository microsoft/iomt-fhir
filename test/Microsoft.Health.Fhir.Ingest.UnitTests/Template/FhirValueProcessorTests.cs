// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirValueProcessorTests
    {
        [Fact]
        public void GivenWrongTemplateType_WhenCreateValue_ThenNotSupportedExceptionThrown_Test()
        {
            var template = new SampledDataFhirValueType();
            var processor = Substitute.ForPartsOf<TestFhirValueProcessor>();
            processor.TestCreateValueImpl(null, null).ReturnsForAnyArgs(string.Empty);

            Assert.Throws<NotSupportedException>(() => processor.CreateValue(template, "1"));

            processor.DidNotReceiveWithAnyArgs().TestCreateValueImpl(null, null);
        }

        [Fact]
        public void GivenWrongTemplateType_WhenMergeValue_ThenNotSupportedExceptionThrown_Test()
        {
            var template = new SampledDataFhirValueType();
            var processor = Substitute.For<TestFhirValueProcessor>();
            processor.TestMergeValueImpl(null, null, null).ReturnsForAnyArgs(string.Empty);

            Assert.Throws<NotSupportedException>(() => processor.MergeValue(template, "1", "2"));

            processor.DidNotReceiveWithAnyArgs().TestMergeValueImpl(null, null, null);
        }

        [Fact]
        public void GivenValidTemplate_WhenCreateValue_ThenImplInvoked_Test()
        {
            var template = new QuantityFhirValueType();
            var processor = Substitute.ForPartsOf<TestFhirValueProcessor>();
            processor.TestCreateValueImpl(null, null).ReturnsForAnyArgs(string.Empty);

            var result = processor.CreateValue(template, "1");
            Assert.Equal(string.Empty, result);

            processor.Received(1).TestCreateValueImpl(template, "1");
        }

        [Fact]
        public void GivenValidTemplate_WhenMergeValue_ThenImplInvoked_Test()
        {
            var template = new QuantityFhirValueType();
            var processor = Substitute.ForPartsOf<TestFhirValueProcessor>();
            processor.TestMergeValueImpl(null, null, null).ReturnsForAnyArgs(string.Empty);

            var result = processor.MergeValue(template, "1", "2");
            Assert.Equal(string.Empty, result);

            processor.Received(1).TestMergeValueImpl(template, "1", "2");
        }

        public class TestFhirValueProcessor : FhirValueProcessor<QuantityFhirValueType, string, string>
        {
            public virtual string TestCreateValueImpl(QuantityFhirValueType template, string inValue)
            {
                return null;
            }

            public virtual string TestMergeValueImpl(QuantityFhirValueType template, string inValue, string existingValue)
            {
                return null;
            }

            protected override string CreateValueImpl(QuantityFhirValueType template, string inValue)
            {
                return TestCreateValueImpl(template, inValue);
            }

            protected override string MergeValueImpl(QuantityFhirValueType template, string inValue, string existingValue)
            {
                return TestMergeValueImpl(template, inValue, existingValue);
            }
        }
    }
}
