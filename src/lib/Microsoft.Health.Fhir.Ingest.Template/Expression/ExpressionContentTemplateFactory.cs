// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Expression
{
    public class ExpressionContentTemplateFactory : HandlerProxyTemplateFactory<TemplateContainer, IContentTemplate>
    {
        private const string TargetTypeName = "ExpressionContent";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message")]
        public override IContentTemplate Create(TemplateContainer jsonTemplate)
        {
            EnsureArg.IsNotNull(jsonTemplate, nameof(jsonTemplate));

            if (!jsonTemplate.MatchTemplateName(TargetTypeName))
            {
                return null;
            }

            if (jsonTemplate.Template?.Type != JTokenType.Object)
            {
                throw new InvalidTemplateException($"Expected an object for the template property value for template type {TargetTypeName}.");
            }

            return jsonTemplate.Template.ToValidTemplate<ExpressionContentTemplate>();
        }
    }
}
