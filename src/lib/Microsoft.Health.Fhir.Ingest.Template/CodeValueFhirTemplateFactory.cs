// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeValueFhirTemplateFactory : HandlerProxyTemplateFactory<TemplateContainer, IFhirTemplate>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message")]
        public override IFhirTemplate Create(TemplateContainer jsonTemplate)
        {
            EnsureArg.IsNotNull(jsonTemplate, nameof(jsonTemplate));

            const string targetTypeName = "CodeValueFhirTemplate";
            if (!jsonTemplate.MatchTemplateName(targetTypeName))
            {
                throw new InvalidTemplateException($"Expected {nameof(jsonTemplate.TemplateType)} value {targetTypeName}, actual {jsonTemplate.TemplateType}.");
            }

            if (jsonTemplate.Template?.Type != JTokenType.Object)
            {
                throw new InvalidTemplateException($"Expected an object for the template property value for template type {targetTypeName}.");
            }

            var codeValueFhirTemplate = jsonTemplate.Template.ToObject<CodeValueFhirTemplate>(GetJsonSerializer());
            if (SerializationErrors?.Count > 0)
            {
                string errorMessage = string.Join(", \n", SerializationErrors);
                throw new InvalidTemplateException($"Failed to deserialize the template content: {errorMessage}");
            }

            return codeValueFhirTemplate;
        }
    }
}
