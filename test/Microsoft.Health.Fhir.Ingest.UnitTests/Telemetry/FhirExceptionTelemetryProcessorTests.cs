// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class FhirExceptionTelemetryProcessorTests
    {
        [Theory]
        [InlineData(typeof(MultipleResourceFoundException<object>), "MultipleObjectFoundException")]
        [InlineData(typeof(PatientDeviceMismatchException), nameof(PatientDeviceMismatchException))]
        [InlineData(typeof(NotSupportedException), nameof(NotSupportedException))]
        [InlineData(typeof(FhirResourceNotFoundException), "DeviceNotFoundException")]
        [InlineData(typeof(ResourceIdentityNotDefinedException), "DeviceIdentityNotDefinedException")]
        [InlineData(typeof(TemplateNotFoundException), nameof(TemplateNotFoundException))]
        [InlineData(typeof(InvalidQuantityFhirValueException), nameof(InvalidQuantityFhirValueException))]
        public void GivenHandledExceptionTypes_WhenHandleExpection_ThenMetricLoggedAndTrueReturned_Test(System.Type exType, string metricName)
        {
            var log = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            var exProcessor = new FhirExceptionTelemetryProcessor();
            var handled = exProcessor.HandleException(ex, log);
            Assert.True(handled);

            log.Received(1).LogError(ex);
            log.Received(1).LogMetric(
                Arg.Is<Metric>(m =>
                string.Equals(m.Name, metricName) &&
                string.Equals(m.Dimensions[DimensionNames.Name], metricName)),
                1);
        }

        [Theory]
        [InlineData(typeof(Exception))]
        public void GivenUnhandledExceptionTypes_WhenHandleExpection_ThenFalseReturned_Test(System.Type exType)
        {
            var log = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            var exProcessor = new FhirExceptionTelemetryProcessor();
            var handled = exProcessor.HandleException(ex, log);
            Assert.False(handled);
        }
    }
}
