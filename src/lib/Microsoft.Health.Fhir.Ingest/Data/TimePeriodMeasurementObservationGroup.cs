// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Globalization;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class TimePeriodMeasurementObservationGroup : SortedObservationGroup
    {
        public const string DateFormat = "yyyyMMddHHmmssZ";
        private readonly (DateTime Start, DateTime End) _bondary;

        public TimePeriodMeasurementObservationGroup((DateTime Start, DateTime End) boundary)
        {
            _bondary = boundary;
        }

        public override (DateTime Start, DateTime End) Boundary => _bondary;

        public override string GetIdSegment()
        {
            var startToken = Boundary.Start.ToString(DateFormat, CultureInfo.InvariantCulture.DateTimeFormat);
            var endToken = Boundary.End.ToString(DateFormat, CultureInfo.InvariantCulture.DateTimeFormat);

            return $"{startToken}.{endToken}";
        }
    }
}