// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class StringFhirValueProcessor : FhirValueProcessor<StringFhirValueType, IObservationData, DataType>
    {
        protected override DataType CreateValueImpl(StringFhirValueType template, IObservationData inValue)
        {
            EnsureArg.IsNotNull(template, nameof(template));
            EnsureArg.IsNotNull(inValue, nameof(inValue));
            IEnumerable<(DateTime, string)> values = EnsureArg.IsNotNull(inValue.Data, nameof(IObservationData.Data));

            return new FhirString(values.Single().Item2);
        }

        protected override DataType MergeValueImpl(StringFhirValueType template, IObservationData inValue, DataType existingValue)
        {
            if (!(existingValue is FhirString))
            {
                throw new NotSupportedException($"Element {nameof(existingValue)} expected to be of type {typeof(FhirString)}.");
            }

            // Only a single value, just replace.
            return CreateValueImpl(template, inValue);
        }
    }
}
