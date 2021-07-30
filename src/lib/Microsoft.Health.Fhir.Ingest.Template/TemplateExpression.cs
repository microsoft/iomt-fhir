// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateExpression
    {
        public TemplateExpression()
        {
        }

        public TemplateExpression(string value, TemplateExpressionLanguage language)
        {
            Value = EnsureArg.IsNotNullOrWhiteSpace(value, nameof(value));
            Language = language;
        }

        [JsonProperty(Required = Required.Always)]
        public virtual string Value { get; set; }

        public TemplateExpressionLanguage? Language { get; set; }
    }
}