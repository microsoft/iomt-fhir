// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;
using Xunit;

namespace Microsoft.Health.Common.UnitTests
{
    public class MetricExtensionTest
    {
        [Fact]
        public void GivenMetric_WhenDimensionAddedAndDimensionAlreadyExists_ThenDimensionValueIsOverwritten_Test()
        {
            // Test the behavior where CreateBaseMetric adds a 'Name' dimension and then a user later calls .AddDimension with 'Name' as an argument
            // Instead of throwing an exception during .AddDimension we should overwrite the dimension value.
            var metricDefinition = new MetricDefinition("testMetricName");

            var metric = MetricExtension.CreateBaseMetric(metricDefinition, Category.Traffic, ConnectorOperation.FHIRConversion);

            Assert.Equal("testMetricName", metric.Dimensions[DimensionNames.Name]);

            metric.AddDimension(DimensionNames.Name, "changedMetricDimensionName");

            Assert.Equal("changedMetricDimensionName", metric.Dimensions[DimensionNames.Name]);
        }
    }
}
