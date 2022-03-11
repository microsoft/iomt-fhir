// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class SampledDataProcessor
    {
        public static SampledDataProcessor Instance => new SampledDataProcessor();

        public virtual string BuildSampledData((DateTime Time, string Value)[] values, DateTime startBoundary, DateTime endBoundary, decimal? samplePeriod)
        {
            EnsureArg.IsNotNull(values, nameof(values));
            EnsureArg.IsNotNull(samplePeriod, nameof(samplePeriod));

            var timeIncrement = Convert.ToDouble(samplePeriod.Value);
            var currentDateTime = startBoundary;
            var nextDateTime = currentDateTime.AddMilliseconds(timeIncrement);
            var i = 0;
            var sb = new StringBuilder();

            while (currentDateTime <= endBoundary)
            {
                string value = null;

                while (i < values.Length && values[i].Time >= currentDateTime && values[i].Time < nextDateTime)
                {
                    value = values[i].Value;
                    i++;
                }

                sb.Append(value ?? "E");
                sb.Append(" ");
                currentDateTime = nextDateTime;
                nextDateTime = nextDateTime.AddMilliseconds(timeIncrement);
            }

            return sb.ToString().TrimEnd();
        }

        public virtual (DateTime Time, string Value)[] SampledDataToTimeValues(string stream, DateTime start, decimal? samplePeriod, bool preserveErrors = false)
        {
            EnsureArg.IsNotNullOrWhiteSpace(stream, nameof(stream));
            EnsureArg.IsNotNull(samplePeriod, nameof(samplePeriod));

            var tokens = stream.Split(' ');
            var timeValues = new List<(DateTime Time, string Value)>(tokens.Length);
            var timeIncrement = Convert.ToDouble(samplePeriod.Value);

            var time = start;
            foreach (var t in tokens)
            {
                if (preserveErrors || t != "E")
                {
                    timeValues.Add((time, t));
                }

                time = time.AddMilliseconds(timeIncrement);
            }

            return timeValues.ToArray();
        }

        public virtual (DateTime Time, string Value)[] MergeData((DateTime Time, string Value)[] data1, (DateTime Time, string Value)[] data2)
        {
            EnsureArg.IsNotNull(data1, nameof(data1));
            EnsureArg.IsNotNull(data2, nameof(data2));

            var output = new List<(DateTime Time, string Value)>(data1.Length + data2.Length);
            var data1Pos = 0;
            var data2Pos = 0;

            while (data1Pos < data1.Length || data2Pos < data2.Length)
            {
                if (data1Pos >= data1.Length)
                {
                    output.Add(data2[data2Pos++]);
                }
                else if (data2Pos >= data2.Length)
                {
                    output.Add(data1[data1Pos++]);
                }
                else if (data1[data1Pos].Time < data2[data2Pos].Time)
                {
                    output.Add(data1[data1Pos++]);
                }
                else if (data1[data1Pos].Time > data2[data2Pos].Time)
                {
                    output.Add(data2[data2Pos++]);
                }
                else
                {
                    // Collision, take one and increment both
                    output.Add(data1[data1Pos++]);
                    data2Pos++;
                }
            }

            return output.ToArray();
        }
    }
}
