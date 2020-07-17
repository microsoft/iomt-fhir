// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    [JsonConverter(typeof(AbstractJsonConverter), typeof(FhirValueType), nameof(FhirValueType.ValueType))]
    public abstract class FhirValueType
    {
        public virtual string ValueName { get; set; }

        public virtual string ValueType { get; set; }
    }
}
