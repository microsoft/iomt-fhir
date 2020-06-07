// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class TimePeriodMeasurementObservationGroupFactory : IObservationGroupFactory<IMeasurementGroup>
    {
        private static readonly IDictionary<ObservationPeriodInterval, Func<DateTime, (DateTime Start, DateTime End)>> BoundaryFunctions = new Dictionary<ObservationPeriodInterval, Func<DateTime, (DateTime Start, DateTime End)>>
        {
            { ObservationPeriodInterval.Single, GetSingleBoundary },
            { ObservationPeriodInterval.Hourly, GetHourlyBoundary },
            { ObservationPeriodInterval.Daily,  GetDailyBoundary },
        };

        private readonly Func<DateTime, (DateTime Start, DateTime End)> _boundaryFunction;

        public TimePeriodMeasurementObservationGroupFactory(ObservationPeriodInterval period)
        {
            _boundaryFunction = BoundaryFunctions[period];
        }

        public IEnumerable<IObservationGroup> Build(IMeasurementGroup input)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            var lookup = new Dictionary<DateTime, IObservationGroup>();
            foreach (var m in input.Data)
            {
                var boundary = GetBoundaryKey(m);
                if (lookup.TryGetValue(boundary.Start, out var grp))
                {
                    grp.AddMeasurement(m);
                }
                else
                {
                    var newGrp = CreateObservationGroup(input, boundary);
                    newGrp.AddMeasurement(m);
                    lookup.Add(boundary.Start, newGrp);
                }
            }

            return lookup.Values;
        }

        public virtual (DateTime Start, DateTime End) GetBoundaryKey(IMeasurement measurement)
        {
            EnsureArg.IsNotNull(measurement);

            return _boundaryFunction(measurement.OccurrenceTimeUtc.ToUniversalTime());
        }

        protected virtual IObservationGroup CreateObservationGroup(IMeasurementGroup group, (DateTime Start, DateTime End) boundary)
        {
            EnsureArg.IsNotNull(group, nameof(group));

            return new TimePeriodMeasurementObservationGroup(boundary)
            {
                Name = group.MeasureType,
            };
        }

        private static (DateTime Start, DateTime End) GetSingleBoundary(DateTime utcDateTime)
        {
            return (utcDateTime, utcDateTime);
        }

        private static (DateTime Start, DateTime End) GetHourlyBoundary(DateTime utcDateTime)
        {
            var start = utcDateTime.Date.AddHours(utcDateTime.Hour);
            var end = start.AddHours(1).AddTicks(-1);

            return (start, end);
        }

        private static (DateTime Start, DateTime End) GetDailyBoundary(DateTime utcDateTime)
        {
            var start = utcDateTime.Date;
            var end = start.AddDays(1).AddTicks(-1);

            return (start, end);
        }
    }
}