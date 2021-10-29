// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Common.Telemetry
{
    public static class MetricExtension
    {
        private static readonly string _nameDimension = DimensionNames.Name;
        private static readonly string _categoryDimension = DimensionNames.Category;
        private static readonly string _operationDimension = DimensionNames.Operation;

        public static Metric CreateBaseMetric(this MetricDefinition iomtMetric, string category, string operation)
        {
            var metricName = iomtMetric.ToString();
            return new Metric(
                metricName,
                new Dictionary<string, object>
                {
                    { _nameDimension, metricName },
                    { _categoryDimension, category },
                    { _operationDimension, operation },
                });
        }

        public static Metric AddDimension(this Metric metric, string dimensionName, string dimensionValue)
        {
            if (string.IsNullOrEmpty(dimensionValue))
            {
                return metric;
            }

            metric.Dimensions.Add(dimensionName, dimensionValue);
            return metric;
        }
    }
}
