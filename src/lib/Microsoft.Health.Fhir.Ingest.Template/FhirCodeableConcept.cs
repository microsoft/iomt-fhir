﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirCodeableConcept
    {
        public string Text { get; set; }

#pragma warning disable CA2227
        public IList<FhirCode> Codes { get; set; }
#pragma warning restore CA2227
    }
}
