// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class MeasurementObservationGroupFactory : IObservationGroupFactory<IMeasurementGroup>
    {
        private static readonly IDictionary<ObservationPeriodInterval, Func<DateTime, (DateTime Start, DateTime End)>> BoundaryFunctions = new Dictionary<ObservationPeriodInterval, Func<DateTime, (DateTime Start, DateTime End)>>
        {
            { ObservationPeriodInterval.Single, GetSingleBoundary },
            { ObservationPeriodInterval.Hourly, GetHourlyBoundary },
            { ObservationPeriodInterval.Daily,  GetDailyBoundary },
        };

        private readonly Func<DateTime, (DateTime Start, DateTime End)> _boundaryFunction;

        public MeasurementObservationGroupFactory(ObservationPeriodInterval period)
        {
            _boundaryFunction = BoundaryFunctions[period];
        }

        public IEnumerable<IObservationGroup> Build(IMeasurementGroup input)
        {
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

        public virtual IObservationGroup CreateObservationGroup(IMeasurementGroup group, (DateTime Start, DateTime End) boundary)
        {
            return new MeasurementObservationGroup
            {
                Name = group.MeasureType,
                Boundary = boundary,
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

        private class MeasurementObservationGroup : IObservationGroup, IComparer<(DateTime Time, string Value)>
        {
            private readonly Dictionary<string, SortedSet<(DateTime Time, string Value)>> _propertyTimeValues = new Dictionary<string, SortedSet<(DateTime Time, string Value)>>();

            public string Name { get; set; }

            public (DateTime Start, DateTime End) Boundary { get; set; }

            public void AddMeasurement(IMeasurement measurement)
            {
                foreach (var mp in measurement.Properties)
                {
                    if (!_propertyTimeValues.TryGetValue(mp.Name, out SortedSet<(DateTime Time, string Value)> values))
                    {
                        values = new SortedSet<(DateTime Time, string Value)>(this);
                        _propertyTimeValues.Add(mp.Name, values);
                    }

                    values.Add((measurement.OccurrenceTimeUtc, mp.Value));
                }
            }

            public int Compare((DateTime Time, string Value) x, (DateTime Time, string Value) y)
            {
                return DateTime.Compare(x.Time, y.Time);
            }

            public IDictionary<string, IEnumerable<(DateTime Time, string Value)>> GetValues() => _propertyTimeValues.ToDictionary(m => m.Key, m => (IEnumerable<(DateTime Time, string Value)>)m.Value);
        }
    }
}