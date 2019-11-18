// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Ingest.Data;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirLookupTemplateProcessorTests
    {
        [Fact]
        public void GivenNoProcessors_WhenCtor_ThenArgumentExceptionThrown_Test()
        {
            Assert.Throws<ArgumentException>(() => new TestFhirLookupTemplateProcessor());
        }

        [Fact]
        public void GivenDefaultCtor_WhenSupportedValueTypeAccessed_ThenValueReturned_Test()
        {
            var p1 = Substitute.For<IFhirTemplateProcessor<IFhirTemplate, string>>();
            p1.SupportedTemplateType.Returns(typeof(IFhirTemplate));
            var processor = new TestFhirLookupTemplateProcessor(p1);
            Assert.NotNull(processor.SupportedTemplateType);
        }

        [Fact]
        public void GivenTemplateWithNoMatchingProcessor_WhenCreateObservation_ThenNotSupportedExceptionThrown_Test()
        {
            var p1 = Substitute.For<IFhirTemplateProcessor<IFhirTemplate, string>>();
            p1.SupportedTemplateType.Returns(typeof(IFhirTemplate));

            var template = Substitute.For<IFhirTemplate>();
            var lookup = Substitute.For<ILookupTemplate<IFhirTemplate>>();
            lookup.GetTemplate("a").Returns(template);
            var observationGroup = Substitute.For<IObservationGroup>();
            observationGroup.Name.Returns("a");

            var processor = new TestFhirLookupTemplateProcessor(p1);
            var ex = Assert.Throws<NotSupportedException>(() => processor.CreateObservation(lookup, observationGroup));

            _ = observationGroup.Received(1).Name;
            lookup.Received(1).GetTemplate("a");
            template.Received(1).GetType();

            p1.DidNotReceiveWithAnyArgs().CreateObservation(null, null);
        }

        [Fact]
        public void GivenTemplateWithNoMatchingProcessor_WhenMergeObservation_ThenNotSupportedExceptionThrown_Test()
        {
            var p1 = Substitute.For<IFhirTemplateProcessor<IFhirTemplate, string>>();
            p1.SupportedTemplateType.Returns(typeof(IFhirTemplate));

            var template = Substitute.For<IFhirTemplate>();
            var lookup = Substitute.For<ILookupTemplate<IFhirTemplate>>();
            lookup.GetTemplate("a").Returns(template);
            var observationGroup = Substitute.For<IObservationGroup>();
            observationGroup.Name.Returns("a");

            var processor = new TestFhirLookupTemplateProcessor(p1);
            var ex = Assert.Throws<NotSupportedException>(() => processor.MergeObservation(lookup, observationGroup, "data"));

            _ = observationGroup.Received(1).Name;
            lookup.Received(1).GetTemplate("a");
            template.Received(1).GetType();

            p1.DidNotReceiveWithAnyArgs().MergeObservation(null, null, null);
        }

        [Fact]
        public void GivenTemplateWithNoMatchingProcessor_WhenCreateObservationGroups_ThenNotSupportedExceptionThrown_Test()
        {
            var p1 = Substitute.For<IFhirTemplateProcessor<IFhirTemplate, string>>();
            p1.SupportedTemplateType.Returns(typeof(IFhirTemplate));

            var template = Substitute.For<IFhirTemplate>();
            var lookup = Substitute.For<ILookupTemplate<IFhirTemplate>>();
            lookup.GetTemplate("a").Returns(template);
            var measurementGroup = Substitute.For<IMeasurementGroup>();
            measurementGroup.MeasureType.Returns("a");

            var processor = new TestFhirLookupTemplateProcessor(p1);
            var ex = Assert.Throws<NotSupportedException>(() => processor.CreateObservationGroups(lookup, measurementGroup));

            _ = measurementGroup.Received(1).MeasureType;
            lookup.Received(1).GetTemplate("a");
            template.Received(1).GetType();

            p1.DidNotReceiveWithAnyArgs().CreateObservationGroups(null, null);
        }

        [Fact]
        public void GivenValidTemplate_WhenCreateObservation_ThenCorrectProcessorInvoked_Test()
        {
            var template = Substitute.For<IFhirTemplate>();
            var lookup = Substitute.For<ILookupTemplate<IFhirTemplate>>();
            lookup.GetTemplate("a").Returns(template);
            var observationGroup = Substitute.For<IObservationGroup>();
            observationGroup.Name.Returns("a");

            var p1 = Substitute.For<IFhirTemplateProcessor<IFhirTemplate, string>>();
            p1.SupportedTemplateType.Returns(template.GetType());
            p1.CreateObservation(null, null).ReturnsForAnyArgs("new");

            var processor = new TestFhirLookupTemplateProcessor(p1);
            var result = processor.CreateObservation(lookup, observationGroup);

            Assert.Equal("new", result);

            _ = observationGroup.Received(1).Name;
            lookup.Received(1).GetTemplate("a");
            template.Received(1).GetType();

            p1.Received(1).CreateObservation(template, observationGroup);
        }

        [Fact]
        public void GivenValidTemplate_WhenMergeObservation_ThenCorrectProcessorInvoked_Test()
        {
            var template = Substitute.For<IFhirTemplate>();
            var lookup = Substitute.For<ILookupTemplate<IFhirTemplate>>();
            lookup.GetTemplate("a").Returns(template);
            var observationGroup = Substitute.For<IObservationGroup>();
            observationGroup.Name.Returns("a");

            var p1 = Substitute.For<IFhirTemplateProcessor<IFhirTemplate, string>>();
            p1.SupportedTemplateType.Returns(template.GetType());
            p1.MergeObservation(null, null, null).ReturnsForAnyArgs("merge");

            var processor = new TestFhirLookupTemplateProcessor(p1);
            var result = processor.MergeObservation(lookup, observationGroup, "data");
            Assert.Equal("merge", result);

            _ = observationGroup.Received(1).Name;
            lookup.Received(1).GetTemplate("a");
            template.Received(1).GetType();

            p1.Received(1).MergeObservation(template, observationGroup, "data");
        }

        [Fact]
        public void GivenValidTemplate_WhenCreateObservationGroups_ThenCorrectProcessorInvoked_Test()
        {
            var template = Substitute.For<IFhirTemplate>();
            var lookup = Substitute.For<ILookupTemplate<IFhirTemplate>>();
            lookup.GetTemplate("a").Returns(template);
            var measurementGroup = Substitute.For<IMeasurementGroup>();
            measurementGroup.MeasureType.Returns("a");

            var data = new IObservationGroup[] { };
            var p1 = Substitute.For<IFhirTemplateProcessor<IFhirTemplate, string>>();
            p1.SupportedTemplateType.Returns(template.GetType());
            p1.CreateObservationGroups(null, null).ReturnsForAnyArgs(data);

            var processor = new TestFhirLookupTemplateProcessor(p1);
            var result = processor.CreateObservationGroups(lookup, measurementGroup);
            Assert.Equal(data, result);

            _ = measurementGroup.Received(1).MeasureType;
            lookup.Received(1).GetTemplate("a");
            template.Received(1).GetType();

            p1.Received(1).CreateObservationGroups(template, measurementGroup);
        }

        private class TestFhirLookupTemplateProcessor : FhirLookupTemplateProcessor<string>
        {
            public TestFhirLookupTemplateProcessor(params IFhirTemplateProcessor<IFhirTemplate, string>[] processors)
                : base(processors)
            {
            }
        }
    }
}
