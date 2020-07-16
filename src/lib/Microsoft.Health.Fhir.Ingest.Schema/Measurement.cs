// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class Measurement : IMeasurement
    {
        public Measurement()
        {
            Properties = new List<MeasurementProperty>();
        }

        public string Type { get; set; }

        public DateTime OccurrenceTimeUtc { get; set; }

        public DateTime? IngestionTimeUtc { get; set; }

        public string DeviceId { get; set; }

        public string PatientId { get; set; }

        public string EncounterId { get; set; }

        public string CorrelationId { get; set; }

#pragma warning disable CA2227
        public IList<MeasurementProperty> Properties { get; set; }
#pragma warning restore CA2227

        IEnumerable<MeasurementProperty> IMeasurement.Properties => Properties;
    }
}
