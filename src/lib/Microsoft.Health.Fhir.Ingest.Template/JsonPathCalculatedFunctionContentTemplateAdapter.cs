// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
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
            TypeMatchExpression = CreateExpression(InnerTemplate.TypeMatchExpression, innerTemplate.GetLineInfoForProperty(nameof(InnerTemplate.TypeMatchExpression)));
            DeviceIdExpression = CreateExpression(InnerTemplate.DeviceIdExpression, innerTemplate.GetLineInfoForProperty(nameof(InnerTemplate.DeviceIdExpression)));
            PatientIdExpression = CreateExpression(InnerTemplate.PatientIdExpression, innerTemplate.GetLineInfoForProperty(nameof(InnerTemplate.PatientIdExpression)));
            EncounterIdExpression = CreateExpression(InnerTemplate.EncounterIdExpression, innerTemplate.GetLineInfoForProperty(nameof(InnerTemplate.EncounterIdExpression)));
            TimestampExpression = CreateExpression(InnerTemplate.TimestampExpression, innerTemplate.GetLineInfoForProperty(nameof(InnerTemplate.TimestampExpression)));
            CorrelationIdExpression = CreateExpression(InnerTemplate.CorrelationIdExpression, innerTemplate.GetLineInfoForProperty(nameof(InnerTemplate.CorrelationIdExpression)));

            if (InnerTemplate.Values != null)
            {
               Values = InnerTemplate.Values.Select(value =>
               new CalculatedFunctionValueExpression()
               {
                   ValueName = value.ValueName,
                   ValueExpression = CreateExpression(value.ValueExpression, value),
                   Required = value.Required,
                   LineNumber = value.LineNumber,
                   LinePosition = value.LinePosition,
                   LineInfoForProperties = value.LineInfoForProperties,
               }).ToList();
            }

            LineNumber = innerTemplate.LineNumber;
            LinePosition = innerTemplate.LinePosition;
            LineInfoForProperties = innerTemplate.LineInfoForProperties;
        }

        public TTemplate InnerTemplate { get; private set; }

        private TemplateExpression CreateExpression(string value, LineInfo lineAwareJsonObject)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (lineAwareJsonObject == null)
                {
                    return new TemplateExpression(value, TemplateExpressionLanguage.JsonPath);
                }

                var templateExpression = new TemplateExpression(value, TemplateExpressionLanguage.JsonPath)
                {
                    LineNumber = lineAwareJsonObject.LineNumber,
                    LinePosition = lineAwareJsonObject.LinePosition,
                };

                templateExpression.LineInfoForProperties[nameof(TemplateExpression.Value)] = new LineInfo()
                {
                    LineNumber = lineAwareJsonObject.LineNumber,
                    LinePosition = lineAwareJsonObject.LinePosition,
                };

                return templateExpression;
            }

            return null;
        }
    }
}
