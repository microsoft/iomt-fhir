// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class JsonPathContentTemplate
    {
        [JsonProperty(Required = Required.Always)]
        public virtual string TypeName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public virtual string TypeMatchExpression { get; set; }

        [JsonProperty(Required = Required.Always)]
        public virtual string DeviceIdExpression { get; set; }

        public virtual string PatientIdExpression { get; set; }

        public virtual string EncounterIdExpression { get; set; }

        [JsonProperty(Required = Required.Always)]
        public virtual string TimestampExpression { get; set; }

        public virtual string CorrelationIdExpression { get; set; }

#pragma warning disable CA2227
        public virtual IList<JsonPathValueExpression> Values { get; set; }
#pragma warning restore CA2227
    }
}
