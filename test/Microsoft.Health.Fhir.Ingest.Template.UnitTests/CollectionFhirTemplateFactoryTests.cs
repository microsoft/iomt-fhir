// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionFhirTemplateFactoryTests
    {
        [Theory]
        [FileData(@"TestInput/data_CollectionFhirTemplateEmpty.json")]
        public void GivenEmptyConfig_WhenCreate_ThenInvalidTemplateException_Test(string json)
        {
            var templateContext = CollectionFhirTemplateFactory.Default.Create(json);
            Assert.NotNull(templateContext);
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionFhirTemplateEmptyWithType.json")]
        public void GivenEmptyTemplateCollection_WhenCreate_ThenTemplateReturned_Test(string json)
        {
            var templateContext = CollectionFhirTemplateFactory.Default.Create(json);
            Assert.NotNull(templateContext);
            Assert.True(templateContext.IsValid(out _));
            templateContext.EnsureValid();
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionFhirTemplateValid.json")]
        public void GivenValidTemplateCollection_WhenCreate_ThenTemplateReturnedWithoutError_Test(string json)
        {
            var templateContext = CollectionFhirTemplateFactory.Default.Create(json);
            Assert.NotNull(templateContext);
            Assert.True(templateContext.IsValid(out _));
            templateContext.EnsureValid();
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionFhirTemplateInvalid.json")]
        public void GivenInvalidTemplateCollection_WhenCreate_ThenValidationShouldFail_Test(string json)
        {
            var templateContext = CollectionFhirTemplateFactory.Default.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out string aggregatedErrors));
            Assert.Contains("Required property 'TypeName' not found in JSON.", aggregatedErrors);
            Assert.Contains("Duplicate template defined for type name", aggregatedErrors);
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionFhirTemplateMixedValidity.json")]
        public void GivenMixedValidityFhirTemplateCollection_WhenCreate_ItShouldWork_Test(string json)
        {
            var templateContext = CollectionFhirTemplateFactory.Default.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out _));

            var codeValueTemplate = templateContext.Template.GetTemplate("bloodpressure") as CodeValueFhirTemplate;
            Assert.NotNull(codeValueTemplate);

            Assert.Equal("bloodpressure", codeValueTemplate.TypeName);
            Assert.Equal(ObservationPeriodInterval.Hourly, codeValueTemplate.PeriodInterval);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionFhirTemplateMultipleMocks.json")]
        public void GivenInputWithRegisteredFactories_WhenCreate_ThenTemplateReturned_Test(string json)
        {
            IFhirTemplate nullReturn = null;

            var templateA = Substitute.For<IFhirTemplate>();
            templateA.TypeName.Returns("A");

            var templateB = Substitute.For<IFhirTemplate>();
            templateB.TypeName.Returns("B");

            var factoryA = Substitute.For<ITemplateFactory<TemplateContainer, IFhirTemplate>>();
            factoryA.Create(Arg.Is<TemplateContainer>(v => v.MatchTemplateName("mockA"))).Returns(templateA);
            factoryA.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockA"))).Returns(nullReturn);

            var factoryB = Substitute.For<ITemplateFactory<TemplateContainer, IFhirTemplate>>();
            factoryB.Create(Arg.Is<TemplateContainer>(v => v.MatchTemplateName("mockB"))).Returns(templateB);
            factoryB.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockB"))).Returns(nullReturn);

            var factory = new CollectionFhirTemplateFactory(factoryA, factoryB);
            var templateContext = factory.Create(json);

            Assert.NotNull(templateContext);
            templateContext.EnsureValid();

            factoryA.ReceivedWithAnyArgs().Create(null);
            factoryB.ReceivedWithAnyArgs().Create(null);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionFhirTemplateMultipleMocks.json")]
        public void GivenInputWithUnregisteredFactories_WhenCreate_ThenException_Test(string json)
        {
            IFhirTemplate nullReturn = null;
            var templateA = Substitute.For<IFhirTemplate>();
            templateA.TypeName.Returns("A");

            var templateC = Substitute.For<IFhirTemplate>();
            templateC.TypeName.Returns("C");

            var factoryA = Substitute.For<ITemplateFactory<TemplateContainer, IFhirTemplate>>();
            factoryA.Create(Arg.Is<TemplateContainer>(v => v.MatchTemplateName("mockA"))).Returns(templateA);
            factoryA.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockA"))).Returns(nullReturn);

            var factoryC = Substitute.For<ITemplateFactory<TemplateContainer, IFhirTemplate>>();
            factoryC.Create(Arg.Is<TemplateContainer>(v => v.MatchTemplateName("mockC"))).Returns(templateC);
            factoryC.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockC"))).Returns(nullReturn);

            var factory = new CollectionFhirTemplateFactory(factoryA, factoryC);
            var templateContext = factory.Create(json);
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
            Assert.False(templateContext.IsValid(out _));

            factoryA.ReceivedWithAnyArgs().Create(null);
            factoryC.ReceivedWithAnyArgs().Create(null);
        }

        [Theory]
        [FileData(@"TestInput/data_InvalidJson.txt")]
        public void GivenBadInputJson_WhenCreate_ThenValidationFailed_Test(string json)
        {
            var templateContext = CollectionFhirTemplateFactory.Default.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out _));
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
        }

        [Theory]
        [FileData(@"TestInput/data_InvalidTemplateType.json")]
        public void GivenMismatchedTemplateTypeInputJson_WhenCreate_ThenValidationFailed_Test(string json)
        {
            var templateContext = CollectionFhirTemplateFactory.Default.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out string error));
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
            Assert.Contains("Expected TemplateType value CollectionFhirTemplate", error);
        }

        [Theory]
        [FileData(@"TestInput/data_InvalidCollectionFhirTemplateWithNoTemplateArray.json")]
        public void GivenNoTemplateArrayInputJson_WhenCreate_ThenValidationFailed_Test(string json)
        {
            var templateContext = CollectionFhirTemplateFactory.Default.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out string error));
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
            Assert.Contains("Expected an array for the template property value for template type CollectionFhirTemplate", error);
        }
    }
}
