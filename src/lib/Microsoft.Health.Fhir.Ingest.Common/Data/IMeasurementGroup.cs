// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public interface IMeasurementGroup
    {
        string MeasureType { get; }

        string DeviceId { get; }

        string PatientId { get; }

        string EncounterId { get; }

        string CorrelationId { get; }

        IEnumerable<IMeasurement> Data { get; }
    }
}
