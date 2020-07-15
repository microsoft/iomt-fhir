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
    public class IotJsonPathContentTemplateFactory : HandlerProxyTemplateFactory<TemplateContainer, IContentTemplate>
    {
        private const string TargetTypeName = "IotJsonPathContentTemplate";

        public override IContentTemplate Create(TemplateContainer jsonTemplate)
        {
            var iotJsonPathContentTemplate = Create(jsonTemplate, out IList<string> _);
            if (TemplateErrors.Any())
            {
                string aggregatedErrorMessage = string.Join(", \n", TemplateErrors);
                throw new InvalidTemplateException($"There were errors found for template type {TargetTypeName}: \n{aggregatedErrorMessage}");
            }

            return iotJsonPathContentTemplate;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message")]

        public override IContentTemplate Create(TemplateContainer jsonTemplate, out IList<string> errors)
        {
            EnsureArg.IsNotNull(jsonTemplate, nameof(jsonTemplate));

            errors = TemplateErrors;

            if (!jsonTemplate.MatchTemplateName(TargetTypeName))
            {
                return null;
            }

            if (jsonTemplate.Template?.Type != JTokenType.Object)
            {
                throw new InvalidTemplateException($"Expected an object for the template property value for template type {TargetTypeName}.");
            }

            var iotJsonPathContentTemplate = jsonTemplate.Template.ToObject<IotJsonPathContentTemplate>(GetJsonSerializer());

            return iotJsonPathContentTemplate;
        }
    }
}
