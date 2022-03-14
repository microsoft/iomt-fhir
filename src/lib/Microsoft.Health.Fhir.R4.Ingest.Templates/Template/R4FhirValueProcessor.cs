// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class R4FhirValueProcessor : CollectionFhirValueProcessor<IObservationData, DataType>
    {
        public R4FhirValueProcessor()
            : base(
                new SampledDataFhirValueProcessor(),
                new CodeableConceptFhirValueProcessor(),
                new QuantityFhirValueProcessor(),
                new StringFhirValueProcessor())
        {
        }
    }
}
