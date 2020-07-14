// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class SampledDataFhirValueType : FhirValueType
    {
        [JsonProperty(Required = Required.Always)]
        public decimal? DefaultPeriod { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Unit { get; set; }
    }
}
