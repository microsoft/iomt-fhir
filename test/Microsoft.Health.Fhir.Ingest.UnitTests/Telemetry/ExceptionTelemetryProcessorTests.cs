// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class ExceptionTelemetryProcessorTests
    {
        [Theory]
        [InlineData(typeof(MultipleResourceFoundException<object>))]
        [InlineData(typeof(PatientDeviceMismatchException))]
        [InlineData(typeof(NotSupportedException))]
        [InlineData(typeof(FhirResourceNotFoundException))]
        [InlineData(typeof(ResourceIdentityNotDefinedException))]
        [InlineData(typeof(TemplateNotFoundException))]
        public void GivenHandledExceptionTypes_WhenHandleExpection_ThenMetricLoggedAndTrueReturned_Test(System.Type exType)
        {
            var log = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            var exProcessor = new ExceptionTelemetryProcessor();
            var handled = exProcessor.HandleException(ex, log, ConnectorStage.FHIRConversion);
            Assert.True(handled);

            log.ReceivedWithAnyArgs(1).LogMetric(null, default(double));
        }

        [Theory]
        [InlineData(typeof(Exception))]
        public void GivenUnhandledExceptionTypes_WhenHandleExpection_ThenNoMetricLoggedAndFalseReturned_Test(System.Type exType)
        {
            var log = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            var exProcessor = new ExceptionTelemetryProcessor();
            var handled = exProcessor.HandleException(ex, log, ConnectorStage.FHIRConversion);
            Assert.False(handled);

            log.DidNotReceiveWithAnyArgs().LogMetric(null, default(double));
        }
    }
}
