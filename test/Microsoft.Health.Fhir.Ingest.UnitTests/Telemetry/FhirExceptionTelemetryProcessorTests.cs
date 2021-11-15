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
        [InlineData(typeof(MultipleResourceFoundException<object>))]
        [InlineData(typeof(PatientDeviceMismatchException))]
        [InlineData(typeof(NotSupportedException))]
        [InlineData(typeof(FhirResourceNotFoundException))]
        [InlineData(typeof(ResourceIdentityNotDefinedException))]
        [InlineData(typeof(TemplateNotFoundException))]
        [InlineData(typeof(InvalidQuantityFhirValueException))]
        public void GivenHandledExceptionTypes_WhenHandleExpection_ThenMetricLoggedAndTrueReturned_Test(System.Type exType)
        {
            var log = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            var exProcessor = new FhirExceptionTelemetryProcessor();
            var handled = exProcessor.HandleException(ex, log);
            Assert.True(handled);

            log.ReceivedWithAnyArgs(1).LogMetric(null, default(double));
        }

        [Theory]
        [InlineData(typeof(Exception))]
        public void GivenUnhandledExceptionTypes_WhenHandleExpection_ThenMetricLoggedAndFalseReturned_Test(System.Type exType)
        {
            var log = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            var exProcessor = new FhirExceptionTelemetryProcessor();
            var handled = exProcessor.HandleException(ex, log);
            Assert.False(handled);

            log.Received(1).LogError(ex);
            log.Received(1).LogMetric(
                Arg.Is<Metric>(m =>
                string.Equals(m.Name, nameof(IomtMetrics.UnhandledException)) &&
                string.Equals(m.Dimensions[DimensionNames.Name], exType.Name)),
                1);
        }
    }
}
