// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CalculatedFunctionContentTemplateExtensionsTests
    {
        [Fact]
        public void When_AllExpressionsAreNull_NoModificationsAreMade()
        {
            var template = new CalculatedFunctionContentTemplate();
            template.EnsureExpressionLanguageIsSet();
            Assert.Null(template.TimestampExpression);
            Assert.Null(template.DeviceIdExpression);
            Assert.Null(template.PatientIdExpression);
            Assert.Null(template.CorrelationIdExpression);
            Assert.Null(template.EncounterIdExpression);
            Assert.Null(template.TypeMatchExpression);
            Assert.Null(template.Values);
        }

        [Fact]
        public void When_DefaultLanguage_IsNotSet_AllBlankExpressionLanguages_SetToJsonPath()
        {
            var template = GenerateContentTemplate(null);

            template.EnsureExpressionLanguageIsSet();
            EnsureAllDefaultLanguagesAreSet(template, TemplateExpressionLanguage.JsonPath);
        }

        [Fact]
        public void When_DefaultLanguage_IsJsonPath_AllBlankExpressionLanguages_SetToJsonPath()
        {
            var template = GenerateContentTemplate(TemplateExpressionLanguage.JsonPath);

            template.EnsureExpressionLanguageIsSet();
            EnsureAllDefaultLanguagesAreSet(template, TemplateExpressionLanguage.JsonPath);
        }

        [Fact]
        public void When_DefaultLanguage_IsSetToJmesPath_AllBlankExpressionLanguages_SetToJmesPath()
        {
            var template = GenerateContentTemplate(TemplateExpressionLanguage.JmesPath);

            template.EnsureExpressionLanguageIsSet();
            EnsureAllDefaultLanguagesAreSet(template, TemplateExpressionLanguage.JmesPath);
        }

        [Fact]
        public void When_DefaultLanguage_IsSetToJmesPath_AllBlankExpressionLanguages_SetToJmesPath_AndPreviouslySetLanguages_RemainTheSame()
        {
            var template = GenerateContentTemplate(TemplateExpressionLanguage.JmesPath);

            template.PatientIdExpression.Language = TemplateExpressionLanguage.JsonPath;
            template.DeviceIdExpression.Language = TemplateExpressionLanguage.JsonPath;

            template.EnsureExpressionLanguageIsSet();

            Assert.Equal(TemplateExpressionLanguage.JmesPath, template.TimestampExpression.Language);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, template.DeviceIdExpression.Language);
            Assert.Equal(TemplateExpressionLanguage.JsonPath, template.PatientIdExpression.Language);
            Assert.Equal(TemplateExpressionLanguage.JmesPath, template.CorrelationIdExpression.Language);
            Assert.Equal(TemplateExpressionLanguage.JmesPath, template.EncounterIdExpression.Language);
            Assert.Equal(TemplateExpressionLanguage.JmesPath, template.TypeMatchExpression.Language);
            Assert.Collection(
                template.Values,
                p => Assert.Equal(TemplateExpressionLanguage.JmesPath, p.ValueExpression.Language));
        }

        private CalculatedFunctionContentTemplate GenerateContentTemplate(TemplateExpressionLanguage? language)
        {
            var template = new CalculatedFunctionContentTemplate
            {
                TypeName = "heartrate",
                TypeMatchExpression = new TemplateExpression(),
                DeviceIdExpression = new TemplateExpression(),
                TimestampExpression = new TemplateExpression(),
                CorrelationIdExpression = new TemplateExpression(),
                EncounterIdExpression = new TemplateExpression(),
                PatientIdExpression = new TemplateExpression(),
                Values = new List<CalculatedFunctionValueExpression>
                    {
                      new CalculatedFunctionValueExpression
                      {
                          ValueName = "hr", ValueExpression = new TemplateExpression(), Required = true,
                      },
                    },
            };

            if (language.HasValue)
            {
                template.DefaultExpressionLanguage = language.Value;
            }

            return template;
        }

        private void EnsureAllDefaultLanguagesAreSet(CalculatedFunctionContentTemplate template, TemplateExpressionLanguage language)
        {
            Assert.Equal(language, template.TimestampExpression.Language);
            Assert.Equal(language, template.DeviceIdExpression.Language);
            Assert.Equal(language, template.PatientIdExpression.Language);
            Assert.Equal(language, template.CorrelationIdExpression.Language);
            Assert.Equal(language, template.EncounterIdExpression.Language);
            Assert.Equal(language, template.TypeMatchExpression.Language);
            Assert.Collection(
                template.Values,
                p => Assert.Equal(language, p.ValueExpression.Language));
        }
    }
}
