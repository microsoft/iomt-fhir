// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionContentTemplateFactoryTests
    {
        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateEmpty.json")]
        public void GivenEmptyConfig_WhenCreate_ThenInvalidTemplateException_Test(string json)
        {
            Assert.Throws<InvalidTemplateException>(() => CollectionContentTemplateFactory.Default.Create(json));
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateEmptyWithType.json")]
        public void GivenEmptyTemplateCollection_WhenCreate_ThenTemplateReturned_Test(string json)
        {
            var template = CollectionContentTemplateFactory.Default.Create(json);
            Assert.NotNull(template);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMultipleMocks.json")]
        public void GivenInputWithMatchingFactories_WhenCreate_ThenTemplateReturned_Test(string json)
        {
            IContentTemplate nullReturn = null;

            var factoryA = Substitute.For<ITemplateFactory<TemplateContainer, IContentTemplate>>();
            factoryA.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockA"))).Returns(nullReturn);

            var factoryB = Substitute.For<ITemplateFactory<TemplateContainer, IContentTemplate>>();
            factoryB.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockB"))).Returns(nullReturn);

            var factory = new CollectionContentTemplateFactory(factoryA, factoryB);
            var template = factory.Create(json);

            Assert.NotNull(template);

            factoryA.ReceivedWithAnyArgs().Create(null);
            factoryB.ReceivedWithAnyArgs().Create(null);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMultipleMocks.json")]
        public void GivenInputWithNoMatchingFactories_WhenCreate_ThenException_Test(string json)
        {
            IContentTemplate nullReturn = null;

            var factoryA = Substitute.For<ITemplateFactory<TemplateContainer, IContentTemplate>>();
            factoryA.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockA"))).Returns(nullReturn);

            var factoryB = Substitute.For<ITemplateFactory<TemplateContainer, IContentTemplate>>();
            factoryB.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockC"))).Returns(nullReturn);

            var factory = new CollectionContentTemplateFactory(factoryA, factoryB);
            Assert.Throws<InvalidTemplateException>(() => factory.Create(json));

            factoryA.ReceivedWithAnyArgs().Create(null);
            factoryB.ReceivedWithAnyArgs().Create(null);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionFhirTemplateMixed.json")]
        public void GivenInputWithMultipleTemplates_WhenCreate_ThenTemplateReturn_Test(string json)
        {
            var template = CollectionContentTemplateFactory.Default.Create(json);
            Assert.NotNull(template);
        }
    }
}
