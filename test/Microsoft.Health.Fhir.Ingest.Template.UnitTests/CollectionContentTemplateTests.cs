// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using Microsoft.Health.Fhir.Ingest.Data;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionContentTemplateTests
    {
        [Fact]
        public void GivenTokenAndNoTemplates_WhenGetMeasurements_EmptyCollectionReturned_Test()
        {
            var template = new CollectionContentTemplate();
            var token = JToken.FromObject(new object());

            var measurements = template.GetMeasurements(token);
            Assert.Empty(measurements);
        }

        [Fact]
        public void GivenTokenAndNoMatchingTemplates_WhenGetMeasurements_EmptyCollectionReturned_Test()
        {
            var template = new CollectionContentTemplate();
            var innerTemplate1 = Substitute.For<IContentTemplate>();
            innerTemplate1.GetMeasurements(null).ReturnsForAnyArgs(t => Enumerable.Empty<Measurement>());
            var innerTemplate2 = Substitute.For<IContentTemplate>();
            innerTemplate2.GetMeasurements(null).ReturnsForAnyArgs(t => Enumerable.Empty<Measurement>());

            template.RegisterTemplate(innerTemplate1)
                .RegisterTemplate(innerTemplate2);

            var token = JToken.FromObject(new object());

            var measurements = template.GetMeasurements(token);
            Assert.Empty(measurements);

            innerTemplate1.Received(1).GetMeasurements(Arg.Is<JToken>(t => t == token));
            innerTemplate2.Received(1).GetMeasurements(Arg.Is<JToken>(t => t == token));
        }

        [Fact]
        public void GivenTokenAndLastTemplateMatches_WhenGetMeasurements_MeasurementsReturned_Test()
        {
            var m1 = new Measurement();

            var template = new CollectionContentTemplate();
            var innerTemplate1 = Substitute.For<IContentTemplate>();
            innerTemplate1.GetMeasurements(null).ReturnsForAnyArgs(t => Enumerable.Empty<Measurement>());
            var innerTemplate2 = Substitute.For<IContentTemplate>();
            innerTemplate2.GetMeasurements(null).ReturnsForAnyArgs(t => new[] { m1 });

            template.RegisterTemplate(innerTemplate1)
                .RegisterTemplate(innerTemplate2);

            var token = JToken.FromObject(new object());

            var measurements = template.GetMeasurements(token);
            Assert.Collection(measurements, m =>
            {
                Assert.Equal(m1, m);
            });

            innerTemplate1.Received(1).GetMeasurements(Arg.Is<JToken>(t => t == token));
            innerTemplate2.Received(1).GetMeasurements(Arg.Is<JToken>(t => t == token));
        }

        [Fact]
        public void GivenTokenAndAllTemplatesMatchAndMultipleResultsGeneratedPerTemplate_WhenGetMeasurements_MeasurementsReturned_Test()
        {
            var m1 = new Measurement();
            var m2 = new Measurement();
            var m3 = new Measurement();

            var template = new CollectionContentTemplate();
            var innerTemplate1 = Substitute.For<IContentTemplate>();
            innerTemplate1.GetMeasurements(null).ReturnsForAnyArgs(t => new[] { m1 });
            var innerTemplate2 = Substitute.For<IContentTemplate>();
            innerTemplate2.GetMeasurements(null).ReturnsForAnyArgs(t => new[] { m2, m3 });

            template.RegisterTemplate(innerTemplate1)
                .RegisterTemplate(innerTemplate2);

            var token = JToken.FromObject(new object());

            var measurements = template.GetMeasurements(token);
            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal(m1, m);
                },
                m =>
                {
                    Assert.Equal(m2, m);
                },
                m =>
                {
                    Assert.Equal(m3, m);
                });

            innerTemplate1.Received(1).GetMeasurements(Arg.Is<JToken>(t => t == token));
            innerTemplate2.Received(1).GetMeasurements(Arg.Is<JToken>(t => t == token));
        }
    }
}
