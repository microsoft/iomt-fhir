// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnsureThat;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Ingest.Service;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class QuantityFhirValueProcessor : FhirValueProcessor<QuantityFhirValueType, IObservationData, DataType>
    {
        protected override DataType CreateValueImpl(QuantityFhirValueType template, IObservationData inValue)
        {
            EnsureArg.IsNotNull(template, nameof(template));
            EnsureArg.IsNotNull(inValue, nameof(inValue));
            IEnumerable<(DateTime, string)> values = EnsureArg.IsNotNull(inValue.Data, nameof(IObservationData.Data));

            decimal value;
            try
            {
                value = decimal.Parse(values.Single().Item2, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                var valuesCount = values.Count();
                var message = valuesCount == 1 ? $"Error encountered processing value: {values.First().Item2}." : $"Expected 1 value. Received {valuesCount}.";
                throw new InvalidQuantityFhirValueException(message, ex);
            }

            return new Quantity
            {
                Value = value,
                Unit = template.Unit,
                System = template.System,
                Code = template.Code,
            };
        }

        protected override DataType MergeValueImpl(QuantityFhirValueType template, IObservationData inValue, DataType existingValue)
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
