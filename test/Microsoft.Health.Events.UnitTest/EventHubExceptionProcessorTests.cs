// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure;
using Azure.Messaging.EventHubs;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Events.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Identity.Client;
using NSubstitute;
using System;
using System.Net.Sockets;
using Xunit;

namespace Microsoft.Health.Events.UnitTest
{
    public class EventHubExceptionProcessorTests
    {
        [Theory]
        [InlineData(typeof(EventHubsException), new object[] { false, "test", EventHubsException.FailureReason.ResourceNotFound }, "EventHubErrorConfigurationError")]
        [InlineData(typeof(EventHubsException), new object[] { false, "test", EventHubsException.FailureReason.ServiceCommunicationProblem }, "EventHubErrorConfigurationError")]
        [InlineData(typeof(EventHubsException), new object[] { false, "test", EventHubsException.FailureReason.ClientClosed }, "EventHubErrorClientClosed")]
        [InlineData(typeof(InvalidOperationException), null, "EventHubErrorConfigurationError")]
        [InlineData(typeof(SocketException), new object[] { SocketError.HostNotFound }, "EventHubErrorConfigurationError")]
        [InlineData(typeof(SocketException), new object[] { SocketError.SocketError }, "EventHubErrorSocketError")]
        [InlineData(typeof(UnauthorizedAccessException), null, "EventHubErrorAuthorizationError")]
        [InlineData(typeof(RequestFailedException), new object[] { "SecretNotFound" }, "ManagedIdentityCredentialNotFound", nameof(ErrorType.AuthenticationError))]
        [InlineData(typeof(MsalServiceException), new object[] { "testErrorCode", "testError" }, "ManagedIdentityAuthenticationErrortestErrorCode", nameof(ErrorType.AuthenticationError))]
        [InlineData(typeof(Exception), null, "EventHubErrorGeneralError")]
        public void GivenExceptionType_WhenProcessExpection_ThenExceptionLoggedAndEventHubErrorMetricLogged_Test(Type exType, object[] param, string expectedErrorMetricName, string expectedErrorTypeName = nameof(ErrorType.EventHubError))
        {
            var logger = Substitute.For<ITelemetryLogger>();
            Exception ex = Activator.CreateInstance(exType, param) as Exception;

            EventHubExceptionProcessor.ProcessException(ex, logger);

            logger.ReceivedWithAnyArgs(1).LogError(ex);
            logger.Received(1).LogMetric(Arg.Is<Metric>(m =>
                m.Name.Equals(expectedErrorMetricName) &&
                ValidateEventHubErrorMetricProperties(m, expectedErrorTypeName)),
                1);
        }

        [Theory]
        [InlineData(typeof(Exception))]
        public void GivenExceptionTypeAndErrorMetricName_WhenProcessExpection_ThenExceptionLoggedAndErrorMetricNameLogged_Test(Type exType)
        {
            var logger = Substitute.For<ITelemetryLogger>();
            var ex = Activator.CreateInstance(exType) as Exception;

            EventHubExceptionProcessor.ProcessException(ex, logger, errorMetricName: EventHubErrorCode.EventHubPartitionInitFailed.ToString());

            logger.Received(1).LogError(ex);
            logger.Received(1).LogMetric(Arg.Is<Metric>(m =>
                m.Name.Equals(EventHubErrorCode.EventHubPartitionInitFailed.ToString()) &&
                ValidateEventHubErrorMetricProperties(m, ErrorType.EventHubError)),
                1);
        }

        [Theory]
        [InlineData(typeof(EventHubsException), new object[] { false, "test", EventHubsException.FailureReason.ResourceNotFound }, typeof(InvalidEventHubException))]
        [InlineData(typeof(EventHubsException), new object[] { false, "test", EventHubsException.FailureReason.ServiceCommunicationProblem }, typeof(InvalidEventHubException))]
        [InlineData(typeof(EventHubsException), new object[] { false, "test", EventHubsException.FailureReason.GeneralError }, typeof(EventHubsException))]
        [InlineData(typeof(InvalidOperationException), null, typeof(InvalidEventHubException))]
        [InlineData(typeof(SocketException), new object[] { SocketError.HostNotFound }, typeof(InvalidEventHubException))]
        [InlineData(typeof(SocketException), new object[] { SocketError.SocketError }, typeof(SocketException))]
        [InlineData(typeof(UnauthorizedAccessException), null, typeof(UnauthorizedAccessEventHubException))]
        [InlineData(typeof(RequestFailedException), new object[] { "SecretNotFound" }, typeof(ManagedIdentityCredentialNotFound))]
        [InlineData(typeof(MsalServiceException), new object[] { "errorCode", "testError" }, typeof(ManagedIdentityAuthenticationError))]
        [InlineData(typeof(Exception), null, typeof(Exception))]
        public void GivenExceptionType_WhenCustomizeException_ThenCustomExceptionTypeReturned_Test(Type exType, object[] param, Type customExType)
        {
            var ex = Activator.CreateInstance(exType, param) as Exception;

            var (customEx, errName) = EventHubExceptionProcessor.CustomizeException(ex);

            Assert.IsType(customExType, customEx);
        }

        private bool ValidateEventHubErrorMetricProperties(Metric metric, string expectedErrorTypeName)
        {
            return metric.Dimensions["Category"].Equals(Category.Errors) &&
                metric.Dimensions["ErrorType"].Equals(expectedErrorTypeName);
        }
    }
}
