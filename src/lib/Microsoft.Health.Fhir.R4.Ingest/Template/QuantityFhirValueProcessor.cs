// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class QuantityFhirValueProcessor : FhirValueProcessor<QuantityFhirValueType, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>
    {
        protected override Element CreateValueImpl(QuantityFhirValueType template, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values) inValue)
        {
            return new Quantity
            {
                Value = decimal.Parse(inValue.values.Single().Item2, CultureInfo.InvariantCulture),
                Unit = template.Unit,
                System = template.System,
                Code = template.Code,
            };
        }

        protected override Element MergeValueImpl(QuantityFhirValueType template, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values) inValue, Element existingValue)
        {
            if (!(existingValue is Quantity))
            {
                throw new NotSupportedException($"Element {nameof(existingValue)} expected to be of type {typeof(Quantity)}.");
            }

            // Only a single value, just replace.
            return CreateValueImpl(template, inValue);
        }
    }
}
