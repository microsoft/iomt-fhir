// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public interface IFhirValueProcessor<TInValue, TOutValue>
    {
        Type SupportedValueType { get; }

        TOutValue CreateValue(FhirValueType template, TInValue inValue);

        TOutValue MergeValue(FhirValueType template, TInValue inValue, TOutValue existingValue);
    }
}
