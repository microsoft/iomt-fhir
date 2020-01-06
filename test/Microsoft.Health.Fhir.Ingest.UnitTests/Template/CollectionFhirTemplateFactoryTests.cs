// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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
            Assert.Throws<InvalidTemplateException>(() => CollectionFhirTemplateFactory.Default.Create(json));
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionFhirTemplateEmptyWithType.json")]
        public void GivenEmptyTemplateCollection_WhenCreate_ThenTemplateReturned_Test(string json)
        {
            var template = CollectionFhirTemplateFactory.Default.Create(json);
            Assert.NotNull(template);
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
            var template = factory.Create(json);

            Assert.NotNull(template);

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
            Assert.Throws<InvalidTemplateException>(() => factory.Create(json));

            factoryA.ReceivedWithAnyArgs().Create(null);
            factoryC.ReceivedWithAnyArgs().Create(null);
        }
    }
}
