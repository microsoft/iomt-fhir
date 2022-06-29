// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Ingest.Template.Serialization;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateExtensionsTests
    {
        [Theory]
        [FileData(@"TestInput/data_CalculatedFunctionContentTemplateValid.json")]
        public void When_ValidateCalculatedContentTemplateContent_Is_ConvertedToTemplateObject_LineNumbersAreAdded(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json, new TemplateContainerJsonConverter());
            var template = templateContainer.Template.ToValidTemplate<CalculatedFunctionContentTemplate>();
            Assert.NotNull(template);
            Assert.True(template.HasLineInfo());
            Assert.Equal(4, template.GetLineInfoForProperty(nameof(template.TypeName)).LineNumber);
        }

        [Theory]
        [FileData(@"TestInput/data_CalculatedFunctionContentTemplateValidInvalidAndMissingMembers.json")]
        public void When_InValidateCalculatedContentTemplateContent_Is_ConvertedToTemplateObject_AggregateExceptionIsThrown(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json, new TemplateContainerJsonConverter());
            var exception = Assert.Throws<AggregateException>(() => templateContainer.Template.ToValidTemplate<CalculatedFunctionContentTemplate>());
            Assert.Collection(
                exception.InnerExceptions,
                p =>
                {
                    var ite = Assert.IsType<InvalidTemplateException>(p);
                    Assert.True(ite.HasLineInfo);
                    Assert.Equal(5, ite.GetLineInfo.LineNumber);
                },
                p =>
                {
                    var ite = Assert.IsType<InvalidTemplateException>(p);
                    Assert.True(ite.HasLineInfo);
                    Assert.Equal(6, ite.GetLineInfo.LineNumber);
                },
                p =>
                {
                    var ite = Assert.IsType<InvalidTemplateException>(p);
                    Assert.True(ite.HasLineInfo);
                    Assert.Equal(3, ite.GetLineInfo.LineNumber);
                },
                p =>
                {
                    var ite = Assert.IsType<InvalidTemplateException>(p);
                    Assert.True(ite.HasLineInfo);
                    Assert.Equal(3, ite.GetLineInfo.LineNumber);
                });
        }
    }
}