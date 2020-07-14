// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class SampledDataFhirValueType : FhirValueType
    {
        public decimal? DefaultPeriod { get; set; }

        public string Unit { get; set; }
    }
}
