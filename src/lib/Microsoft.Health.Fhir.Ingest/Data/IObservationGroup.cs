// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public interface IObservationGroup
    {
        string Name { get; }

        (DateTime Start, DateTime End) Boundary { get; }

        void AddMeasurement(IMeasurement measurement);

        IDictionary<string, IEnumerable<(DateTime Time, string Value)>> GetValues();

        string GetIdSegment();
    }
}
