// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;

namespace Microsoft.Health.Common.Telemetry
{
    public class MetricDefinition
    {
        public MetricDefinition(string metricName)
        {
            EnsureArg.IsNotNullOrEmpty(metricName, nameof(metricName));
            MetricName = metricName;
        }

        public string MetricName { get; }

        public override string ToString()
        {
            return MetricName;
        }
    }
}
