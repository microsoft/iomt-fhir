// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests.Samples
{
    public class TestModelProjection
    {
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

        [JsonProperty("diastolic")]
        public double Diastolic { get; set; }

        [JsonProperty("systolic")]
        public double Systolic { get; set; }

        [JsonProperty("heartRate")]
        public double HeartRate { get; set; }

        [JsonProperty("oxygenSaturation")]
        public double OxygenSaturation { get; set; }
    }
}
