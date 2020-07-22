// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionContentTemplate : IContentTemplate
    {
        private readonly IList<string> _templateErrors = new List<string>();

        private readonly IList<IContentTemplate> _templates = new List<IContentTemplate>(10);

        public IList<string> TemplateErrors => _templateErrors;

        public CollectionContentTemplate RegisterTemplate(IContentTemplate contentTemplate)
        {
            EnsureArg.IsNotNull(contentTemplate, nameof(contentTemplate));

            _templates.Add(contentTemplate);

            return this;
        }

        public IEnumerable<Measurement> GetMeasurements(JToken token)
        {
            return _templates.SelectMany(t => t.GetMeasurements(token));
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> aggregatedResult = new List<ValidationResult>();
            _templateErrors.ToList()
                .ForEach(e => aggregatedResult.Add(new ValidationResult(e)));

            _templates.ToList()
                .ForEach(t => aggregatedResult.AddRange(t.Validate(validationContext)));

            return aggregatedResult;
        }
    }
}
