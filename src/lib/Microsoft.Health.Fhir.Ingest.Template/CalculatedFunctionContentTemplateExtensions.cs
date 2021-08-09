// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public static class CalculatedFunctionContentTemplateExtensions
    {
        /// <summary>
        /// Iterates over all expressions belonging to the content template. If the expression language has not been set it will be set
        /// to default language of the content template.
        /// </summary>
        /// <param name="template">The template to retrieve expressions from</param>
        public static CalculatedFunctionContentTemplate EnsureExpressionLanguageIsSet(this CalculatedFunctionContentTemplate template)
        {
            EnsureArg.IsNotNull(template, nameof(template));

            foreach (var tuple in template.GetExpressions())
            {
                if (tuple.expression != null)
                {
                    tuple.expression.Language ??= template.DefaultExpressionLanguage;
                }
            }

            return template;
        }

        private static IEnumerable<(string name, TemplateExpression expression)> GetExpressions(this CalculatedFunctionContentTemplate template)
        {
            EnsureArg.IsNotNull(template, nameof(template));

            yield return (nameof(template.TypeMatchExpression), template.TypeMatchExpression);
            yield return (nameof(template.DeviceIdExpression), template.DeviceIdExpression);
            yield return (nameof(template.PatientIdExpression), template.PatientIdExpression);
            yield return (nameof(template.EncounterIdExpression), template.EncounterIdExpression);
            yield return (nameof(template.TimestampExpression), template.TimestampExpression);
            yield return (nameof(template.CorrelationIdExpression), template.CorrelationIdExpression);

            if (template.Values != null)
            {
                foreach (CalculatedFunctionValueExpression value in template.Values)
                {
                    yield return (value.ValueName, value.ValueExpression);
                }
            }
        }
    }
}
