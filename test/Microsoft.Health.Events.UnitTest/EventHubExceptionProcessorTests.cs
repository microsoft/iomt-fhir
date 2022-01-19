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
using System.Collections.Generic;
using System.Net.Sockets;
using Xunit;

namespace Microsoft.Health.Events.UnitTest
{
    public class EventHubExceptionProcessorTests
    {
        private static readonly Exception _eventResourceNotFoundEx = new EventHubsException(false, "test", EventHubsException.FailureReason.ResourceNotFound);
        private static readonly Exception _eventServiceCommunicationProblemEx = new EventHubsException(false, "test", EventHubsException.FailureReason.ServiceCommunicationProblem);
        private static readonly Exception _eventClientClosedEx = new EventHubsException(false, "test", EventHubsException.FailureReason.ClientClosed);
        private static readonly Exception _invalidConsumerGroupEx = new InvalidOperationException("ConsumerGroup");
        private static readonly Exception _invalidEx = new InvalidOperationException();
        private static readonly Exception _socketHostNotFoundEx = new SocketException((int)SocketError.HostNotFound);
        private static readonly Exception _socketSocketErrorEx = new SocketException((int)SocketError.SocketError);
        private static readonly Exception _unauthEx = new UnauthorizedAccessException();
        private static readonly Exception _requestEx = new RequestFailedException("SecretNotFound");
        private static readonly Exception _msalEx = new MsalServiceException("testErrorCode", "testError");
        private static readonly Exception _ex = new Exception();

        public static IEnumerable<object[]> ProcessExceptionData =>
            new List<object[]>
            {
                new object[] { _eventResourceNotFoundEx, "EventHubErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _eventServiceCommunicationProblemEx, "EventHubErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _eventClientClosedEx, "EventHubErrorClientClosed" },
                new object[] { _invalidConsumerGroupEx, "EventHubErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _invalidEx, "EventHubErrorInvalidOperationError" },
                new object[] { _socketHostNotFoundEx, "EventHubErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _socketSocketErrorEx, "EventHubErrorSocketError" },
                new object[] { _unauthEx, "EventHubErrorAuthorizationError", nameof(ErrorSource.User) },
                new object[] { _requestEx, "ManagedIdentityCredentialNotFound", nameof(ErrorSource.User), nameof(ErrorType.AuthenticationError) },
                new object[] { _msalEx, "ManagedIdentityAuthenticationErrortestErrorCode", nameof(ErrorSource.User), nameof(ErrorType.AuthenticationError) },
                new object[] { _ex, "EventHubErrorGeneralError" },
            };

        public static IEnumerable<object[]> CustomizeExceptionData =>
            new List<object[]>
            {
                new object[] { _eventResourceNotFoundEx, typeof(InvalidEventHubException) },
                new object[] { _eventServiceCommunicationProblemEx, typeof(InvalidEventHubException) },
                new object[] { _eventClientClosedEx, typeof(EventHubsException) },
                new object[] { _invalidConsumerGroupEx, typeof(InvalidEventHubException) },
                new object[] { _invalidEx, typeof(InvalidOperationException) },
                new object[] { _socketHostNotFoundEx, typeof(InvalidEventHubException) },
                new object[] { _socketSocketErrorEx, typeof(SocketException) },
                new object[] { _unauthEx, typeof(UnauthorizedAccessEventHubException) },
                new object[] { _requestEx, typeof(ManagedIdentityCredentialNotFound) },
                new object[] { _msalEx, typeof(ManagedIdentityAuthenticationError) },
                new object[] { _ex, typeof(Exception) },
            };

        [Theory]
        [MemberData(nameof(ProcessExceptionData))]
        public void GivenExceptionType_WhenProcessException_ThenExceptionLoggedAndEventHubErrorMetricLogged_Test(Exception ex, string expectedErrorMetricName, string expectedErrorSource = null, string expectedErrorTypeName = nameof(ErrorType.EventHubError))
        {
            var logger = Substitute.For<ITelemetryLogger>();

            EventHubExceptionProcessor.ProcessException(ex, logger);

            logger.ReceivedWithAnyArgs(1).LogError(ex);
            logger.Received(1).LogMetric(Arg.Is<Metric>(m =>
                ValidateEventHubErrorMetricProperties(m, expectedErrorMetricName, expectedErrorTypeName, expectedErrorSource)),
                1);
        }

        [Theory]
        [InlineData(typeof(Exception))]
        public void GivenExceptionTypeAndErrorMetricName_WhenProcessException_ThenExceptionLoggedAndErrorMetricNameLogged_Test(Type exType)
        {
            var ex = Activator.CreateInstance(exType) as Exception;
            var logger = Substitute.For<ITelemetryLogger>();
            var expectedErrorMetricName = EventHubErrorCode.EventHubPartitionInitFailed.ToString();

            EventHubExceptionProcessor.ProcessException(ex, logger, errorMetricName: expectedErrorMetricName);

            logger.Received(1).LogError(ex);
            logger.Received(1).LogMetric(Arg.Is<Metric>(m =>
                ValidateEventHubErrorMetricProperties(m, expectedErrorMetricName, ErrorType.EventHubError, null)),
                1);
        }

        [Theory]
        [MemberData(nameof(CustomizeExceptionData))]
        public void GivenExceptionType_WhenCustomizeException_ThenCustomExceptionTypeReturned_Test(Exception ex, Type customExType)
        {
            var (customEx, errName) = EventHubExceptionProcessor.CustomizeException(ex);

            Assert.IsType(customExType, customEx);
        }

        private bool ValidateEventHubErrorMetricProperties(Metric metric, string expectedErrorMetricName, string expectedErrorTypeName, string expectedErrorSource)
        {
            return metric.Name.Equals(expectedErrorMetricName) &&
                metric.Dimensions[DimensionNames.Name].Equals(expectedErrorMetricName) &&
                metric.Dimensions[DimensionNames.Operation].Equals(ConnectorOperation.Setup) &&
                metric.Dimensions[DimensionNames.Category].Equals(Category.Errors) &&
                metric.Dimensions[DimensionNames.ErrorType].Equals(expectedErrorTypeName) &&
                metric.Dimensions[DimensionNames.ErrorSeverity].Equals(ErrorSeverity.Critical) &&
                (string.IsNullOrWhiteSpace(expectedErrorSource) ? !metric.Dimensions.ContainsKey(DimensionNames.ErrorSource) : metric.Dimensions[DimensionNames.ErrorSource].Equals(expectedErrorSource));
        }
    }
}
