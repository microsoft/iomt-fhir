// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeValueFhirTemplate : FhirTemplate
    {
#pragma warning disable CA2227
        public virtual IList<FhirCodeableConcept> Category { get; set; }
#pragma warning restore CA2227

#pragma warning disable CA2227
        public virtual IList<FhirCode> Codes { get; set; }
#pragma warning restore CA2227

        public virtual FhirValueType Value { get; set; }

#pragma warning disable CA2227
        public virtual IList<CodeValueMapping> Components { get; set; }
#pragma warning restore CA2227
    }
}
