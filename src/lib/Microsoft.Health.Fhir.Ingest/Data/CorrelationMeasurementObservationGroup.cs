// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class CorrelationMeasurementObservationGroup : SortedObservationGroup
    {
        private readonly string _correlationId;
        private DateTime _start = DateTime.MaxValue;
        private DateTime _end = DateTime.MinValue;

        public CorrelationMeasurementObservationGroup(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new CorrelationIdNotDefinedException();
            }

            _correlationId = correlationId;
        }

        public override (DateTime Start, DateTime End) Boundary => (_start, _end);

        public override void AddMeasurement(IMeasurement measurement)
        {
            EnsureArg.IsNotNull(measurement, nameof(measurement));

            base.AddMeasurement(measurement);

            if (measurement.OccurrenceTimeUtc < _start)
            {
                _start = measurement.OccurrenceTimeUtc;
            }

            if (measurement.OccurrenceTimeUtc > _end)
            {
                _end = measurement.OccurrenceTimeUtc;
            }
        }

        public override string GetIdSegment()
        {
            return _correlationId;
        }
    }
}