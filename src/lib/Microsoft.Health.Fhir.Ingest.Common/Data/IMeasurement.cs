// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public interface IMeasurement
    {
        string Type { get; }

        DateTime OccurrenceTimeUtc { get; }

        DateTime? IngestionTimeUtc { get; }

        string DeviceId { get; }

        string PatientId { get; }

        string EncounterId { get; }

        string CorrelationId { get; }

        IEnumerable<MeasurementProperty> Properties { get; }
    }
}
