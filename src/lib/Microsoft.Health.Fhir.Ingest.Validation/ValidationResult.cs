// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Data;
using Newtonsoft.Json.Linq;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Validation
{
    public class ValidationResult
    {
        public JToken DeviceEvent { get; set; }

        public IList<Measurement> Measurements { get; set; } = new List<Measurement>();

        public IList<Model.Observation> Observations { get; set; } = new List<Model.Observation>();

        public IList<string> Exceptions { get; set; } = new List<string>();

        public IList<string> Warnings { get; set; } = new List<string>();

        public long SequenceNumber { get; set; }
    }
}