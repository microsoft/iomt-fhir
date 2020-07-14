// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirCode
    {
        [JsonProperty(Required = Required.Always)]
        public string Code { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string System { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Display { get; set; }
    }
}
