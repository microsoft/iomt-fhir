// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirLookupTemplate : ILookupTemplate<IFhirTemplate>
    {
        private readonly IList<string> _serializationErrors = new List<string>();

        private readonly IDictionary<string, IFhirTemplate> _templates = new Dictionary<string, IFhirTemplate>(StringComparer.InvariantCultureIgnoreCase);

        public IList<string> SerializationErrors => _serializationErrors;

        public FhirLookupTemplate RegisterTemplate(IFhirTemplate fhirTemplate)
        {
            EnsureArg.IsNotNull(fhirTemplate, nameof(fhirTemplate));

            if (fhirTemplate?.TypeName != null)
            {
                if (!_templates.ContainsKey(fhirTemplate.TypeName))
                {
                    _templates.Add(fhirTemplate.TypeName, fhirTemplate);
                }
                else
                {
                    _serializationErrors.Add($"Duplicate template defined for type name: '{fhirTemplate.TypeName}'");
                }
            }
            else
            {
                fhirTemplate.SerializationErrors.ToList()
                    .ForEach(e => _serializationErrors.Add(e));
            }

            return this;
        }

        public IFhirTemplate GetTemplate(string type)
        {
            EnsureArg.IsNotNullOrWhiteSpace(type, nameof(type));
            if (!_templates.TryGetValue(type, out var template))
            {
                throw new TemplateNotFoundException(type);
            }

            return template;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> aggregatedResult = new List<ValidationResult>();
            _serializationErrors.ToList()
                .ForEach(e => aggregatedResult.Add(new ValidationResult(e)));

            _templates.Values.ToList()
                .ForEach(t => aggregatedResult.AddRange(t.Validate(validationContext)));

            return aggregatedResult;
        }
    }
}
