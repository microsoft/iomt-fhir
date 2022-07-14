// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class NormalizationExceptionTelemetryProcessorTests
    {
        [Theory]
        [InlineData(typeof(IncompatibleDataException))]
        [InlineData(typeof(InvalidDataFormatException))]
        public void GivenHandledExceptionTypes_WhenHandleExpection_ThenMetricLoggedAndTrueReturned_Test(System.Type exType)
        {
            var log = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;
            var exceptionConfig = Substitute.For<IExceptionTelemetryProcessorConfig>();

            var exProcessor = new NormalizationExceptionTelemetryProcessor(exceptionConfig);
            var handled = exProcessor.HandleException(ex, log);
            Assert.True(handled);

            log.Received(1).LogError(ex);
            log.Received(1).LogMetric(
                Arg.Is<Metric>(m =>
                string.Equals(m.Name, exType.Name) &&
                string.Equals(m.Dimensions[DimensionNames.Name], exType.Name)),
                1);
        }

        [Theory]
        [InlineData(typeof(Exception))]
        public void GivenUnhandledExceptionTypes_WhenHandleExpection_ThenMetricLoggedAndFalseReturned_Test(System.Type exType)
        {
            var log = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;
            var exceptionConfig = Substitute.For<IExceptionTelemetryProcessorConfig>();

            var exProcessor = new NormalizationExceptionTelemetryProcessor(exceptionConfig);
            var handled = exProcessor.HandleException(ex, log);
            Assert.False(handled);
        }
    }
}
