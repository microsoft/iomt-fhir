// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public abstract class SortedObservationGroup : IObservationGroup, IComparer<(DateTime Time, string Value)>
    {
        private readonly Dictionary<string, SortedSet<(DateTime Time, string Value)>> _propertyTimeValues = new Dictionary<string, SortedSet<(DateTime Time, string Value)>>();

        public virtual string Name { get; set; }

        public abstract (DateTime Start, DateTime End) Boundary { get; }

        protected IDictionary<string, SortedSet<(DateTime Time, string Value)>> PropertyTimeValues => _propertyTimeValues;

        public virtual void AddMeasurement(IMeasurement measurement)
        {
            EnsureArg.IsNotNull(measurement);

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

        public abstract string GetIdSegment();

        public virtual IDictionary<string, IEnumerable<(DateTime Time, string Value)>> GetValues() => _propertyTimeValues.ToDictionary(m => m.Key, m => (IEnumerable<(DateTime Time, string Value)>)m.Value);

        public int Compare((DateTime Time, string Value) x, (DateTime Time, string Value) y)
        {
            return DateTime.Compare(x.Time, y.Time);
        }
    }
}