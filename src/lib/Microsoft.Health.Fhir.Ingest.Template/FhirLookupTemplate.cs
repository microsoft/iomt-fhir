// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirLookupTemplate : ILookupTemplate<IFhirTemplate>
    {
        private readonly IDictionary<string, IFhirTemplate> _templates = new Dictionary<string, IFhirTemplate>(StringComparer.InvariantCultureIgnoreCase);

        public IReadOnlyList<IFhirTemplate> Templates => _templates.Values.ToList().AsReadOnly();

        public FhirLookupTemplate RegisterTemplate(IFhirTemplate fhirTemplate)
        {
            EnsureArg.IsNotNull(fhirTemplate, nameof(fhirTemplate));

            if (!string.IsNullOrWhiteSpace(fhirTemplate.TypeName))
            {
                if (!_templates.ContainsKey(fhirTemplate.TypeName))
                {
                    _templates.Add(fhirTemplate.TypeName, fhirTemplate);
                }
                else
                {
                    throw new InvalidTemplateException($"Duplicate template defined for type name: '{fhirTemplate.TypeName}'");
                }
            }
            else
            {
                throw new InvalidTemplateException($"Empty type name is not allowed: '{fhirTemplate.TypeName}'");
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
    }
}
