// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public interface IFhirTemplateProcessor<TTemplate, TObservation>
        where TTemplate : class
    {
        Type SupportedTemplateType { get; }

        TObservation CreateObservation(TTemplate template, IObservationGroup observationGroup);

        TObservation MergeObservation(TTemplate template, IObservationGroup observationGroup, TObservation existingObservation);

        IEnumerable<IObservationGroup> CreateObservationGroups(TTemplate template, IMeasurementGroup measurementGroup);
    }
}
