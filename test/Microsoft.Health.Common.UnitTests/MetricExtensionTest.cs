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
        public void GivenMetric_WhenDimensionAddedAndDimensionAlreadyExists_ThenDimensionValueIsNotOverwritten_Test()
        {
            // Test the behavior where CreateBaseMetric adds a 'Name' dimension and then a user later calls .AddDimension with 'Name' as an argument
            // Instead of throwing an exception during .AddDimension we should not add the dimension, log an error trace, but not throw an exception.
            var metricDefinition = new MetricDefinition("testMetricName");

            var metric = MetricExtension.CreateBaseMetric(metricDefinition, Category.Traffic, ConnectorOperation.FHIRConversion);

            Assert.Equal("testMetricName", metric.Dimensions[DimensionNames.Name]);

            metric.AddDimension(DimensionNames.Name, "attemptToChangeMetricDimensionValue");

            // Assert metric dimension value unchanged
            Assert.Equal("testMetricName", metric.Dimensions[DimensionNames.Name]);
        }
    }
}
