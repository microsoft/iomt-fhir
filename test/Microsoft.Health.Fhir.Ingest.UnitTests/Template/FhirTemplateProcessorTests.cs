// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Data;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirTemplateProcessorTests
    {
        [Fact]
        public void GivenWrongTemplateType_WhenCreateObservation_ThenNotSupportedExceptionThrown_Test()
        {
            var template = Substitute.For<IFhirTemplate>();
            var processor = Substitute.ForPartsOf<TestFhirTemplateProcessor>();
            var observationGroup = Substitute.For<IObservationGroup>();
            processor.TestCreateObseravtionImpl(null, null).ReturnsForAnyArgs(string.Empty);

            Assert.Throws<NotSupportedException>(() => processor.CreateObservation(template, observationGroup));

            processor.DidNotReceiveWithAnyArgs().TestCreateObseravtionImpl(null, null);
        }

        [Fact]
        public void GivenWrongTemplateType_WhenMergeObservation_ThenNotSupportedExceptionThrown_Test()
        {
            var template = Substitute.For<IFhirTemplate>();
            var processor = Substitute.ForPartsOf<TestFhirTemplateProcessor>();
            var observationGroup = Substitute.For<IObservationGroup>();
            processor.TestMergeObservationImpl(null, null, null).ReturnsForAnyArgs(string.Empty);

            Assert.Throws<NotSupportedException>(() => processor.MergeObservation(template, observationGroup, "1"));

            processor.DidNotReceiveWithAnyArgs().TestMergeObservationImpl(null, null, null);
        }

        [Fact]
        public void GivenWrongTemplateType_WhenCreateObservationGroups_ThenNotSupportedExceptionThrown_Test()
        {
            var template = Substitute.For<IFhirTemplate>();
            var processor = Substitute.ForPartsOf<TestFhirTemplateProcessor>();
            var measurementGroup = Substitute.For<IMeasurementGroup>();
            processor.TestCreateObservationGroupsImpl(null, null).ReturnsForAnyArgs(Substitute.For<IEnumerable<IObservationGroup>>());

            Assert.Throws<NotSupportedException>(() => processor.CreateObservationGroups(template, measurementGroup));

            processor.DidNotReceiveWithAnyArgs().TestCreateObservationGroupsImpl(null, null);
        }

        [Fact]
        public void GivenValidTemplate_WhenCreateObservation_ThenImplInvoked_Test()
        {
            var template = new CodeValueFhirTemplate();
            var processor = Substitute.ForPartsOf<TestFhirTemplateProcessor>();
            var observationGroup = Substitute.For<IObservationGroup>();
            processor.TestCreateObseravtionImpl(null, null).ReturnsForAnyArgs(string.Empty);

            var result = processor.CreateObservation(template, observationGroup);
            Assert.Equal(string.Empty, result);

            processor.Received(1).TestCreateObseravtionImpl(template, observationGroup);
        }

        [Fact]
        public void GivenValidTemplate_WhenMergeObservation_ThenImplInvoked_Test()
        {
            var template = new CodeValueFhirTemplate();
            var processor = Substitute.ForPartsOf<TestFhirTemplateProcessor>();
            var observationGroup = Substitute.For<IObservationGroup>();
            processor.TestMergeObservationImpl(null, null, null).ReturnsForAnyArgs(string.Empty);

            var result = processor.MergeObservation(template, observationGroup, "1");
            Assert.Equal(string.Empty, result);

            processor.Received(1).TestMergeObservationImpl(template, observationGroup, "1");
        }

        [Fact]
        public void GivenValidTemplateType_WhenCreateObservationGroups_ThenImplInvoked_Test()
        {
            var template = new CodeValueFhirTemplate();
            var processor = Substitute.ForPartsOf<TestFhirTemplateProcessor>();
            var measurementGroup = Substitute.For<IMeasurementGroup>();
            var expected = Substitute.For<IEnumerable<IObservationGroup>>();
            processor.TestCreateObservationGroupsImpl(null, null).ReturnsForAnyArgs(expected);

            var result = processor.CreateObservationGroups(template, measurementGroup);
            Assert.Equal(expected, result);

            processor.Received(1).TestCreateObservationGroupsImpl(template, measurementGroup);
        }

        public class TestFhirTemplateProcessor : FhirTemplateProcessor<CodeValueFhirTemplate, string>
        {
            public virtual string TestCreateObseravtionImpl(CodeValueFhirTemplate template, IObservationGroup observationGroup)
            {
                return null;
            }

            public virtual string TestMergeObservationImpl(CodeValueFhirTemplate template, IObservationGroup observationGroup, string existingObservation)
            {
                return null;
            }

            public virtual IEnumerable<IObservationGroup> TestCreateObservationGroupsImpl(CodeValueFhirTemplate template, IMeasurementGroup measurementGroup)
            {
                return null;
            }

            protected override string CreateObservationImpl(CodeValueFhirTemplate template, IObservationGroup observationGroup)
            {
                return TestCreateObseravtionImpl(template, observationGroup);
            }

            protected override string MergeObservationImpl(CodeValueFhirTemplate template, IObservationGroup observationGroup, string existingObservation)
            {
                return TestMergeObservationImpl(template, observationGroup, existingObservation);
            }

            protected override IEnumerable<IObservationGroup> CreateObservationGroupsImpl(CodeValueFhirTemplate template, IMeasurementGroup measurementGroup)
            {
                return TestCreateObservationGroupsImpl(template, measurementGroup);
            }
        }
    }
}
