// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CalculatedFunctionValueExpression
    {
        [JsonProperty(Required = Newtonsoft.Json.Required.Always)]
        public string ValueName { get; set; }

        [JsonProperty(Required = Newtonsoft.Json.Required.Always)]
        [JsonConverter(typeof(TemplateExpressionJsonConverter))]
        public TemplateExpression ValueExpression { get; set; }

        public bool Required { get; set; }
    }
}