// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using EnsureThat;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeableConceptFhirValueProcessor : FhirValueProcessor<CodeableConceptFhirValueType, IObservationData, DataType>
    {
        protected override DataType CreateValueImpl(CodeableConceptFhirValueType template, IObservationData inValue)
        {
            // Values for codeable concepts currently have no meaning. The existence of the measurement means the code applies.

            EnsureArg.IsNotNull(template, nameof(template));

            return new CodeableConcept
            {
                Text = template.Text,
                Coding = template.Codes.Select(t =>
                new Coding
                {
                    Code = t.Code,
                    Display = t.Display,
                    System = t.System,
                }).ToList(),
            };
        }

        protected override DataType MergeValueImpl(CodeableConceptFhirValueType template, IObservationData inValue, DataType existingValue)
        {
            if (!(existingValue is CodeableConcept))
            {
                throw new NotSupportedException($"Element {nameof(existingValue)} expected to be of type {typeof(CodeableConcept)}.");
            }

            // Currently no way to merge codeable concepts. Just replace.
            return CreateValueImpl(template, inValue);
        }
    }
}
