// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests.Samples;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests
{
    public class TemplateCollectionGeneratorTests
    {
        private readonly TestTemplateCollectionGenerator _generator;

        public TemplateCollectionGeneratorTests()
        {
            _generator = new TestTemplateCollectionGenerator();
        }

        [Fact]
        public async Task GivenNoTemplatesGenerated_WhenGenerateTemplateCollectionCalled_EmptyCollectionReturned()
        {
            JObject collection = await _generator.GenerateTemplateCollection(new List<TestModel>() { new TestModel() }, CancellationToken.None);

            Assert.Equal(TemplateCollectionType.CollectionContent.ToString(), collection["templateType"].ToString());
            Assert.Empty(collection["template"] as JArray);
        }

        [Fact]
        public async Task GivenTemplateGenerated_WhenGenerateTemplateCollectionCalled_ExpectedCollectionReturned()
        {
            _generator.TemplateResponses.Add(JObject.Parse("{\"template\":{\"typeName\":\"testTemplate\"}}"));

            JObject collection = await _generator.GenerateTemplateCollection(new List<TestModel>() { new TestModel() }, CancellationToken.None);

            Assert.Equal(TemplateCollectionType.CollectionContent.ToString(), collection["templateType"].ToString());
            Assert.Single(collection["template"] as JArray);
        }

        [Fact]
        public async Task GivenDuplicateTemplatesGenerated_WhenGenerateTemplateCollectionCalled_ExpectedCollectionReturned()
        {
            _generator.TemplateResponses.Add(JObject.Parse("{\"template\":{\"typeName\":\"testTemplate\"}}"));
            _generator.TemplateResponses.Add(JObject.Parse("{\"template\":{\"typeName\":\"testTemplate\"}}"));

            JObject collection = await _generator.GenerateTemplateCollection(new List<TestModel>() { new TestModel(), new TestModel() }, CancellationToken.None);

            Assert.Equal(TemplateCollectionType.CollectionContent.ToString(), collection["templateType"].ToString());
            Assert.Single(collection["template"] as JArray);
        }

        [Fact]
        public async Task GivenDifferentTemplatesGeneratedWithSameTypeName_WhenGenerateTemplateCollectionCalled_ThrowsException()
        {
            _generator.TemplateResponses.Add(JObject.Parse("{\"template\":{\"typeName\":\"testTemplate\"}}"));
            _generator.TemplateResponses.Add(JObject.Parse("{\"template\":{\"typeName\":\"testTemplate\"},\"different\":\"value\"}"));

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _generator.GenerateTemplateCollection(new List<TestModel>() { new TestModel(), new TestModel() }, CancellationToken.None));
        }

        [Fact]
        public async Task GivenMultipleUniqueTemplatesGenerated_WhenGenerateTemplateCollectionCalled_ExpectedCollectionReturned()
        {
            _generator.TemplateResponses.Add(JObject.Parse("{\"template\":{\"typeName\":\"testTemplate1\"}}"));
            _generator.TemplateResponses.Add(JObject.Parse("{\"template\":{\"typeName\":\"testTemplate2\"}}"));

            JObject collection = await _generator.GenerateTemplateCollection(new List<TestModel>() { new TestModel(), new TestModel() }, CancellationToken.None);

            Assert.Equal(TemplateCollectionType.CollectionContent.ToString(), collection["templateType"].ToString());
            Assert.Equal(2, (collection["template"] as JArray).Count);
        }
    }
}
