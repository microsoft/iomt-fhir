// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class FhirLookupTemplate : ILookupTemplate<IFhirTemplate>
    {
        private readonly IDictionary<string, IFhirTemplate> _templates = new Dictionary<string, IFhirTemplate>(StringComparer.InvariantCultureIgnoreCase);

        public FhirLookupTemplate RegisterTemplate(IFhirTemplate fhirTemplate)
        {
            EnsureArg.IsNotNull(fhirTemplate, nameof(fhirTemplate));
            if (!_templates.TryAdd(fhirTemplate.TypeName, fhirTemplate))
            {
                throw new InvalidOperationException($"Duplicate template defined for {fhirTemplate.TypeName}");
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
