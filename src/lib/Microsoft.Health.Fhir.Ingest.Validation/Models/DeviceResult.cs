// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Data;
using Newtonsoft.Json.Linq;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Validation.Models
{
    public class DeviceResult : IResult
    {
        public JToken DeviceEvent { get; set; }

        public IList<Measurement> Measurements { get; set; } = new List<Measurement>();

        public IList<Model.Observation> Observations { get; set; } = new List<Model.Observation>();

        public IList<ValidationError> Exceptions { get; set; } = new List<ValidationError>();

        /// <summary>
        /// Indicates how many Device Events produced the associated Exception. This value is only set when aggregating DeviceEvent results. Otherwise
        /// it will be zero.
        /// </summary>
        public int AggregatedCount { get; set; }
    }
}
