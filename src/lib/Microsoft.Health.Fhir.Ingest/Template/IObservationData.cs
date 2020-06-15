// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public interface IObservationData
    {
        (DateTime start, DateTime end) ObservationPeriod { get; }

        (DateTime start, DateTime end) DataPeriod { get; }

        IEnumerable<(DateTime, string)> Data { get; }
    }
}
