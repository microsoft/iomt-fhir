// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class MeasurementGroup : IMeasurementGroup
    {
        public string DeviceId { get; set; }

        public string PatientId { get; set; }

        public string EncounterId { get; set; }

        public string CorrelationId { get; set; }

        public DateTime WindowTime { get; set; }

        public string MeasureType { get; set; }

        public long Count { get; set; }

#pragma warning disable CA2227
        public IList<Measurement> Data { get; set; }
#pragma warning restore CA2227

        IEnumerable<IMeasurement> IMeasurementGroup.Data => Data;
    }
}
