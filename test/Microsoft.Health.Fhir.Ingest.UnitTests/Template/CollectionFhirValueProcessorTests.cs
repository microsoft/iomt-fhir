// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionFhirValueProcessorTests
    {
        [Fact]
        public void GivenNoProcessors_WhenCtor_ThenArgumentExceptionThrown_Test()
        {
            Assert.Throws<ArgumentException>(() => new TestCollectionFhirValueProcessor());
        }

        [Fact]
        public void GivenDefaultCtor_WhenSupportedValueTypeAccessed_ThenValueReturned_Test()
        {
            var p1 = Substitute.For<IFhirValueProcessor<string, string>>();
            p1.SupportedValueType.Returns(typeof(TestTypeA));
            var processor = new TestCollectionFhirValueProcessor(p1);
            Assert.NotNull(processor.SupportedValueType);
        }

        [Fact]
        public void GivenTemplateWithNoMatchingProcessor_WhenCreateValue_ThenNotSupportedExceptionThrown_Test()
        {
            var p1 = Substitute.For<IFhirValueProcessor<string, string>>();
            p1.SupportedValueType.Returns(typeof(TestTypeA));

            var processor = new TestCollectionFhirValueProcessor(p1);

            var ex = Assert.Throws<NotSupportedException>(() => processor.CreateValue(new TestTypeB(), "1"));
        }

        [Fact]
        public void GivenTemplateWithNoMatchingProcessor_WhenMergeValue_ThenNotSupportedExceptionThrown_Test()
        {
            var p1 = Substitute.For<IFhirValueProcessor<string, string>>();
            p1.SupportedValueType.Returns(typeof(TestTypeA));

            var processor = new TestCollectionFhirValueProcessor(p1);

            var ex = Assert.Throws<NotSupportedException>(() => processor.MergeValue(new TestTypeB(), "e", "1"));
        }

        [Fact]
        public void GivenValidTemplate_WhenCreateValue_ThenCorrectProcessorInvoked_Test()
        {
            var p1 = Substitute.For<IFhirValueProcessor<string, string>>();
            p1.SupportedValueType.Returns(typeof(TestTypeA));

            var p2 = Substitute.For<IFhirValueProcessor<string, string>>();
            p2.SupportedValueType.Returns(typeof(TestTypeB));

            var processor = new TestCollectionFhirValueProcessor(p1, p2);

            var vt1 = new TestTypeA();
            var vt2 = new TestTypeB();

            processor.CreateValue(vt1, "a");
            p1.Received(1).CreateValue(vt1, "a");
            p2.DidNotReceiveWithAnyArgs().CreateValue(null, null);

            p1.ClearReceivedCalls();
            processor.CreateValue(vt2, "b");
            p2.Received(1).CreateValue(vt2, "b");
            p1.DidNotReceiveWithAnyArgs().CreateValue(null, null);
        }

        [Fact]
        public void GivenValidTemplate_WhenMergeValue_ThenCorrectProcessorInvoked_Test()
        {
            var p1 = Substitute.For<IFhirValueProcessor<string, string>>();
            p1.SupportedValueType.Returns(typeof(TestTypeA));

            var p2 = Substitute.For<IFhirValueProcessor<string, string>>();
            p2.SupportedValueType.Returns(typeof(TestTypeB));

            var processor = new TestCollectionFhirValueProcessor(p1, p2);

            var vt1 = new TestTypeA();
            var vt2 = new TestTypeB();

            processor.MergeValue(vt1, "e", "a");
            p1.Received(1).MergeValue(vt1, "e", "a");
            p2.DidNotReceiveWithAnyArgs().MergeValue(null, null, null);

            p1.ClearReceivedCalls();
            processor.MergeValue(vt2, "e", "b");
            p2.Received(1).MergeValue(vt2, "e", "b");
            p1.DidNotReceiveWithAnyArgs().MergeValue(null, null, null);
        }

        private class TestCollectionFhirValueProcessor : CollectionFhirValueProcessor<string, string>
        {
            public TestCollectionFhirValueProcessor(params IFhirValueProcessor<string, string>[] valueProcessors)
                : base(valueProcessors)
            {
            }
        }

        private class TestTypeA : FhirValueType
        {
        }

        private class TestTypeB : FhirValueType
        {
        }
    }
}
