// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public class CalculatedFunctionContentTemplate
    {
        [JsonProperty(Required = Required.Always)]
        public virtual string TypeName { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression TypeMatchExpression { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression DeviceIdExpression { get; set; }

        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression PatientIdExpression { get; set; }

        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression EncounterIdExpression { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression TimestampExpression { get; set; }

        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression CorrelationIdExpression { get; set; }

#pragma warning disable CA2227
        public virtual IList<CalculatedFunctionValueExpression> Values { get; set; }
#pragma warning restore CA2227

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ExpressionLanguage DefaultExpressionLanguage { get; set; } = ExpressionLanguage.JsonPath;
    }
}
