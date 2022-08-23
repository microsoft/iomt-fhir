// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests.Samples
{
    public class TestModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("device")]
        public string Device { get; set; }

        [JsonProperty("patient")]
        public string Patient { get; set; }

        [JsonProperty("encounter")]
        public string Encounter { get; set; }

        [JsonProperty("correlation")]
        public string Correlation { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("values")]
        public Dictionary<string, object> Values { get; set; }
    }
}
