// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateContext<TTemplate> : ITemplateContext<TTemplate>
        where TTemplate : class
    {
        private readonly ICollection<TemplateError> _errors = new Collection<TemplateError>();

        public TTemplate Template { get; set; }

        public ICollection<TemplateError> Errors
        {
            get { return _errors; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return _errors
                .Select(e => new ValidationResult(e.Message));
        }
    }
}
