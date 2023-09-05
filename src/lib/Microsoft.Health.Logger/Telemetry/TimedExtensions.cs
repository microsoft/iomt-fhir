// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Logging.Telemetry
{
    public static class TimedExtensions
    {
        /// <summary>
        /// Creates an ITimed object around the provided <see cref="ITelemetryLogger"/> and tracks the duration till the ITimed object is disposed and emits the duration to the <see cref="ITelemetryLogger"/>
        /// </summary>
        /// <param name="logger">The <see cref="ITelemetryLogger"/> the duration is emitted to when the ITimed object is disposed</param>
        /// <param name="metric">The metric to record the tracked duration to.</param>
        /// <returns>ITimed object wrapping the provided log</returns>
        public static ITimed TrackDuration(this ITelemetryLogger logger, Metric metric)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(metric, nameof(metric));

            return new TimedLog
            {
                Stopwatch = Stopwatch.StartNew(),
                Log = logger,
                Metric = metric,
            };
        }

        private struct TimedLog : ITimed
        {
            public Stopwatch Stopwatch { get; set; }

            public Metric Metric { get; set; }

            public ITelemetryLogger Log { get; set; }

            public TimeSpan Elapsed => Stopwatch.Elapsed;

            public ITimed Record()
            {
                return Record(Metric);
            }

            public ITimed Record(Metric metric)
            {
                EnsureArg.IsNotNull(metric, nameof(metric));
                Log.LogMetric(metric, Elapsed.TotalMilliseconds);
                return this;
            }

            public void Dispose()
            {
                Stopwatch.Stop();
                Record();
            }
        }
    }
}
