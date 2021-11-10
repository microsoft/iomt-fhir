// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;
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
        [InlineData(typeof(InvalidEventHubException), new object[] { "test", null, "test" })]
        [InlineData(typeof(UnauthorizedAccessEventHubException), new object[] { "test", null, "test", "test" })]
        [InlineData(typeof(ManagedIdentityCredentialNotFound), new object[] { "test", null})]
        [InlineData(typeof(ManagedIdentityAuthenticationError), new object[] { "test", null, "test" })]
        public void GivenCustomExceptionTypeWithoutHelpLink_WhenHandleExpection_ThenHandledAndMetricLogged_Test(Type exType, object[] param)
        {
            var ex = Activator.CreateInstance(exType, param) as Exception;

            GivenException_WhenHandleExpection_ThenHandledAndMetricLogged_Test(ex);
        }

        [Theory]
        [InlineData(typeof(Exception))]
        public void GivenSystemExceptionType_WhenHandleExpection_ThenUnhandledAndNoMetricLogged_Test(Type exType)
        {
            var logger = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;
            var processor = new EventHubConfigurationExceptionTelemetryProcessor();

            var handled = processor.HandleException(ex, logger, ConnectorOperation.Setup);

            Assert.False(handled);
            logger.DidNotReceiveWithAnyArgs().LogMetric(null, default(double));
        }

        private void GivenException_WhenHandleExpection_ThenHandledAndMetricLogged_Test(Exception ex)
        {
            var logger = Substitute.For<ITelemetryLogger>();
            var processor = new EventHubConfigurationExceptionTelemetryProcessor();

            var handled = processor.HandleException(ex, logger, ConnectorOperation.Setup);

            Assert.True(handled);
            logger.ReceivedWithAnyArgs(1).LogMetric(null, default(double));
        }
    }
}
