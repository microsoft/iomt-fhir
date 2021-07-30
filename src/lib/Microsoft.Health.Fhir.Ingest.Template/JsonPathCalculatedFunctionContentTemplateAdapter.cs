// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class JsonPathCalculatedFunctionContentTemplateAdapter<TTemplate> : CalculatedFunctionContentTemplate
        where TTemplate : JsonPathContentTemplate, new()
    {
        public JsonPathCalculatedFunctionContentTemplateAdapter(TTemplate innerTemplate)
        {
            InnerTemplate = EnsureArg.IsNotNull(innerTemplate, nameof(innerTemplate));

            TypeName = InnerTemplate.TypeName;
            TypeMatchExpression = CreateExpression(InnerTemplate.TypeMatchExpression);
            DeviceIdExpression = CreateExpression(InnerTemplate.DeviceIdExpression);
            PatientIdExpression = CreateExpression(InnerTemplate.PatientIdExpression);
            EncounterIdExpression = CreateExpression(InnerTemplate.EncounterIdExpression);
            TimestampExpression = CreateExpression(InnerTemplate.TimestampExpression);
            CorrelationIdExpression = CreateExpression(InnerTemplate.CorrelationIdExpression);
            Values = InnerTemplate.Values.Select(value =>
               new CalculatedFunctionValueExpression()
               {
                   ValueName = value.ValueName,
                   Value = value.ValueExpression,
                   Language = TemplateExpressionLanguage.JsonPath,
                   Required = value.Required,
               }).ToList();
        }

        public TTemplate InnerTemplate { get; private set; }

        private TemplateExpression CreateExpression(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return new TemplateExpression(value, TemplateExpressionLanguage.JsonPath);
            }

            return null;
        }
    }
}
