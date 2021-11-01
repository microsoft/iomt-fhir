// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using System;
using Xunit;

namespace Microsoft.Health.Events.UnitTest
{
    public class EventHubConfigurationExceptionTelemetryProcessorTests
    {
        [Theory]
        [InlineData(typeof(InvalidEventHubException))]
        [InlineData(typeof(UnauthorizedAccessEventHubException))]
        public void GivenHandledExceptionType_WhenHandleExpection_ThenMetricLoggedAndTrueReturned_Test(Type exType)
        {
            var logger = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            var processor = new EventHubConfigurationExceptionTelemetryProcessor();
            var handled = processor.HandleException(ex, logger, ConnectorOperation.Setup);
            Assert.True(handled);

            logger.ReceivedWithAnyArgs(1).LogMetric(null, default(double));
        }

        [Theory]
        [InlineData(typeof(Exception))]
        public void GivenUnhandledExceptionType_WhenHandleExpection_ThenNoMetricLoggedAndFalseReturned_Test(Type exType)
        {
            var logger = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            var processor = new EventHubConfigurationExceptionTelemetryProcessor();
            var handled = processor.HandleException(ex, logger, ConnectorOperation.Setup);
            Assert.False(handled);

            logger.DidNotReceiveWithAnyArgs().LogMetric(null, default(double));
        }
    }
}
