// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class R4FhirLookupTemplateProcessor : FhirLookupTemplateProcessor<Observation>
    {
        public R4FhirLookupTemplateProcessor()
            : base(new CodeValueFhirTemplateProcessor())
        {
        }
    }
}
