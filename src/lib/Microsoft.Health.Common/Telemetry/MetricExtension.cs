// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Telemetry
{
    public static class MetricExtension
    {
        public static Metric AddDimension(this Metric metric, string dimensionName, string dimensionValue)
        {
            metric.Dimensions.Add(dimensionName, dimensionValue);
            return metric;
        }
    }
}
