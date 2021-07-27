// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionContentTemplateFactoryTests
    {
        private CollectionContentTemplateFactory _collectionContentTemplateFactory;

        public CollectionContentTemplateFactoryTests()
        {
            var logger = Substitute.For<ITelemetryLogger>();

            _collectionContentTemplateFactory = new CollectionContentTemplateFactory(
                new JsonPathContentTemplateFactory(),
                new IotJsonPathContentTemplateFactory(),
                new IotCentralJsonPathContentTemplateFactory(),
                new CalculatedFunctionContentTemplateFactory(new TemplateExpressionEvaluatorFactory(), logger));
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateEmpty.json")]
        public void GivenEmptyConfig_WhenCreate_ThenInvalidTemplateException_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateEmptyWithType.json")]
        public void GivenEmptyTemplateCollection_WhenCreate_ThenTemplateReturned_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            templateContext.EnsureValid();

            IEnumerable<ValidationResult> validationResult = templateContext.Validate(new ValidationContext(templateContext));
            Assert.Empty(validationResult);
            Assert.NotNull(templateContext.Template);
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
            var templateContext = factory.Create(json);

            Assert.NotNull(templateContext);

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
            factoryB.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockB"))).Returns(nullReturn);

            var factory = new CollectionContentTemplateFactory(factoryA, factoryB);
            factory.Create(json);

            factoryA.ReceivedWithAnyArgs().Create(null);
            factoryB.ReceivedWithAnyArgs().Create(null);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMixed.json")]
        public void GivenInputWithMultipleTemplates_WhenCreate_ThenTemplateReturn_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.True(templateContext.IsValid(out _));
            templateContext.EnsureValid();
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateInvalid.json")]
        public void GivenInvalidTemplateCollection_WhenCreate_ThenValidationShouldFail_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out string errors));
            Assert.Contains("Required property 'DeviceIdExpression' not found in JSON.", errors);
            Assert.Contains("Required property 'TimestampExpression' not found in JSON.", errors);
            Assert.Contains("Required property 'TypeMatchExpression' not found in JSON.", errors);
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMixedValidity.json")]
        public void GivenMixedValidityTemplateCollection_WhenCreate_ItShouldWork_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out _));

            var token = JToken.FromObject(new { heartrate = "60", device = "myHrDevice", date = DateTime.UtcNow });
            var measurements = templateContext.Template.GetMeasurements(token);

            Assert.Collection(measurements, m =>
            {
                Assert.Equal("heartrate", m.Type);
                Assert.Equal("myHrDevice", m.DeviceId);
                Assert.Collection(m.Properties, p =>
                {
                    Assert.Equal("hr", p.Name);
                    Assert.Equal("60", p.Value);
                });
            });
        }

        [Theory]
        [FileData(@"TestInput/data_InvalidJson.txt")]
        public void GivenBadInputJson_WhenCreate_ThenValidationFailed_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out _));
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
        }

        [Theory]
        [FileData(@"TestInput/data_InvalidTemplateType.json")]
        public void GivenMismatchedTemplateTypeInputJson_WhenCreate_ThenValidationFailed_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out string error));
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
            Assert.Contains("Expected TemplateType value CollectionContentTemplate", error);
        }

        [Theory]
        [FileData(@"TestInput/data_InvalidCollectionContentTemplateWithNoTemplateArray.json")]
        public void GivenNoTemplateArrayInputJson_WhenCreate_ThenValidationFailed_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out string error));
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
            Assert.Contains("Expected an array for the template property value for template type CollectionContentTemplate.", error);
        }
    }
}
