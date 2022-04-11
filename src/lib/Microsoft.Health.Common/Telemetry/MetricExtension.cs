// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using EnsureThat;

namespace Microsoft.Health.Common.Telemetry
{
    public static class MetricExtension
    {
        private static readonly string _nameDimension = DimensionNames.Name;
        private static readonly string _categoryDimension = DimensionNames.Category;
        private static readonly string _operationDimension = DimensionNames.Operation;

        public static Metric CreateBaseMetric(this MetricDefinition iomtMetric, string category, string operation)
        {
            EnsureArg.IsNotNull(iomtMetric);
            EnsureArg.IsNotNullOrWhiteSpace(category, nameof(category));
            EnsureArg.IsNotNullOrWhiteSpace(operation, nameof(operation));

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
            if (string.IsNullOrWhiteSpace(dimensionValue))
            {
                return metric;
            }

            metric.Dimensions.Add(dimensionName, dimensionValue);
            return metric;
        }

        public static Metric ToErrorMetric(this string metricName, string operation, string errorType, string errorSeverity, string errorSource = "", string errorName = "")
        {
            EnsureArg.IsNotNullOrWhiteSpace(metricName, nameof(metricName));
            EnsureArg.IsNotNullOrWhiteSpace(operation, nameof(operation));
            EnsureArg.IsNotNullOrWhiteSpace(errorType, nameof(errorType));
            EnsureArg.IsNotNullOrWhiteSpace(errorSeverity, nameof(errorSeverity));

            return new Metric(
                metricName,
                new Dictionary<string, object>
                {
                    { DimensionNames.Name, string.IsNullOrWhiteSpace(errorName) ? metricName : errorName },
                    { DimensionNames.Category, Category.Errors },
                    { DimensionNames.ErrorType, errorType },
                    { DimensionNames.ErrorSeverity, errorSeverity },
                    { DimensionNames.Operation, operation },
                })
                .AddDimension(DimensionNames.ErrorSource, errorSource);
        }
    }
}
