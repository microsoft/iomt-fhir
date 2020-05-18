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
    public class StringFhirValueProcessor : FhirValueProcessor<StringFhirValueType, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>
    {
        protected override Element CreateValueImpl(StringFhirValueType template, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values) inValue)
        {
            EnsureArg.IsNotNull(template, nameof(template));

            return new FhirString(inValue.values.Single().Item2);
        }

        protected override Element MergeValueImpl(StringFhirValueType template, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values) inValue, Element existingValue)
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
