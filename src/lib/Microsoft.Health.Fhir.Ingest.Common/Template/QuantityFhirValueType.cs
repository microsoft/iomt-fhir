// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class QuantityFhirValueType : FhirValueType
    {
        public string Unit { get; set; }

        public string System { get; set; }

        public string Code { get; set; }
    }
}
