// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Health.Fhir.Ingest.Template
{

    public class TemplateContext<TTemplate> : ITemplateContext<TTemplate>
        where TTemplate : class
    {
        public TTemplate Template { get; set;  }

        public ICollection<Exception> Errors { get; } = new Collection<Exception>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
