// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeableConceptFhirValueProcessor : FhirValueProcessor<CodeableConceptFhirValueType, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>
    {
        protected override Element CreateValueImpl(CodeableConceptFhirValueType template, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values) inValue)
        {
            // Values for codeable concepts currently have no meaning. The existance of the measurement means the code applies.

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

        protected override Element MergeValueImpl(CodeableConceptFhirValueType template, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values) inValue, Element existingValue)
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
