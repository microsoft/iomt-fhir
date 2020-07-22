// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public abstract class FhirTemplate : IFhirTemplate
    {
        private readonly IList<string> _serializationErrors = new List<string>();

        public IList<string> SerializationErrors => _serializationErrors;

        [JsonProperty(Required = Required.Always)]
        public virtual string TypeName { get; set; }

        public virtual ObservationPeriodInterval PeriodInterval { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return SerializationErrors
                .Select(e => new ValidationResult(e))
                .ToList();
        }
    }
}
