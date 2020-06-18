// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class ObservationData : IObservationData
    {
        public (DateTime start, DateTime end) ObservationPeriod { get; set; }

        public (DateTime start, DateTime end) DataPeriod { get; set; }

        public IEnumerable<(DateTime, string)> Data { get; set; }
    }
}
