// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirLookupTemplateTests
    {
        [Fact]
        public void GivenDuplicateTemplates_WhenRegisterTemplate_InvalidOperationExceptionThrown_Test()
        {
            var template = new FhirLookupTemplate();

            var templateA = Substitute.For<IFhirTemplate>();
            templateA.TypeName.Returns("test");

            var templateB = Substitute.For<IFhirTemplate>();
            templateB.TypeName.Returns("test");

            template.RegisterTemplate(templateA);
            Assert.Throws<InvalidOperationException>(() => template.RegisterTemplate(templateB));
        }

        [Fact]
        public void GivenUnregisteredTemplate_WhenGetTemplate_TemplateNotFoundExceptionThrown_Test()
        {
            var template = new FhirLookupTemplate();

            var templateA = Substitute.For<IFhirTemplate>();
            templateA.TypeName.Returns("a");

            var templateB = Substitute.For<IFhirTemplate>();
            templateB.TypeName.Returns("b");

            template.RegisterTemplate(templateA);
            template.RegisterTemplate(templateB);

            Assert.Throws<TemplateNotFoundException>(() => template.GetTemplate("c"));
        }

        [Fact]
        public void GivenRegisteredTemplate_WhenGetTemplate_TemplateReturned_Test()
        {
            var template = new FhirLookupTemplate();

            var templateA = Substitute.For<IFhirTemplate>();
            templateA.TypeName.Returns("a");

            var templateB = Substitute.For<IFhirTemplate>();
            templateB.TypeName.Returns("b");

            template.RegisterTemplate(templateA);
            template.RegisterTemplate(templateB);

            var returnTemplate = template.GetTemplate("a");

            Assert.Equal(templateA, returnTemplate);

            returnTemplate = template.GetTemplate("b");
            Assert.Equal(templateB, returnTemplate);
        }
    }
}
