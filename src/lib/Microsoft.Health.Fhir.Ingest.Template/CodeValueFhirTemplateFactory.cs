// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeValueFhirTemplateFactory : HandlerProxyTemplateFactory<TemplateContainer, IFhirTemplate>
    {
        private const string TargetTypeName = "CodeValueFhirTemplate";

        public override IFhirTemplate Create(TemplateContainer jsonTemplate)
        {
            var codeValueFhirTemplate = Create(jsonTemplate, out _);
            if (TemplateErrors.Any())
            {
                string aggregatedErrorMessage = string.Join(", \n", TemplateErrors);
                throw new InvalidTemplateException($"There were errors found for template type {TargetTypeName}: \n{aggregatedErrorMessage}");
            }

            return codeValueFhirTemplate;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message")]
        public override IFhirTemplate Create(TemplateContainer jsonTemplate, out IList<string> errors)
        {
            EnsureArg.IsNotNull(jsonTemplate, nameof(jsonTemplate));

            if (!jsonTemplate.MatchTemplateName(TargetTypeName))
            {
                throw new InvalidTemplateException($"Expected {nameof(jsonTemplate.TemplateType)} value {TargetTypeName}, actual {jsonTemplate.TemplateType}.");
            }

            if (jsonTemplate.Template?.Type != JTokenType.Object)
            {
                throw new InvalidTemplateException($"Expected an object for the template property value for template type {TargetTypeName}.");
            }

            var codeValueFhirTemplate = jsonTemplate.Template.ToObject<CodeValueFhirTemplate>(GetJsonSerializer());

            errors = TemplateErrors;
            return codeValueFhirTemplate;
        }
    }
}
