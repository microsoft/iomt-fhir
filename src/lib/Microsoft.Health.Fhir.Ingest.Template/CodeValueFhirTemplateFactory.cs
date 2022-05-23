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
            var lineInfo = jsonTemplate.GetLineInfoForProperty(nameof(TemplateContainer.TemplateType));
            if (!jsonTemplate.MatchTemplateName(targetTypeName))
            {
                throw new InvalidTemplateException($"Expected {nameof(jsonTemplate.TemplateType)} value {targetTypeName}, actual {jsonTemplate.TemplateType}.", lineInfo);
            }

            if (jsonTemplate.Template?.Type != JTokenType.Object)
            {
                throw new InvalidTemplateException($"Expected an object for the template property value for template type {targetTypeName}.", lineInfo);
            }

            return jsonTemplate.Template.ToValidTemplate<CodeValueFhirTemplate>();
        }
    }
}
