// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net.Sockets;
using Azure.Messaging.EventHubs;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.Telemetry.Exceptions
{
    public static class EventHubExceptionTelemetryProcessor
    {
        private static readonly EventHubConfigurationExceptionTelemetryProcessor _exceptionTelemetryProcessor = new EventHubConfigurationExceptionTelemetryProcessor();

        public static void ProcessException(
            Exception exception,
            ITelemetryLogger logger,
            string errorMetricName = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            var customException = CustomizeException(exception);

            logger.LogError(customException);

            if (customException.Equals(exception))
            {
                logger.LogMetric(
                    EventMetrics.HandledException(
                        errorMetricName ?? GetErrorMetricName(exception),
                        ConnectorOperation.Setup),
                    1);
            }
            else
            {
                _exceptionTelemetryProcessor.HandleException(customException, logger, ConnectorOperation.Setup);
            }
        }

        public static Exception CustomizeException(Exception exception)
        {
            EnsureArg.IsNotNull(exception, nameof(exception));

            string message = exception.Message;

            switch (exception)
            {
                case EventHubsException _:
                    EventHubsException eventHubsException = (EventHubsException)exception;
                    switch (eventHubsException.Reason)
                    {
                        case EventHubsException.FailureReason.ConsumerDisconnected:
                            message = "Verify that the provided Event Hub's consumer group is not already receiving data for another IoT connector or Azure resource.";
                            break;
                        case EventHubsException.FailureReason.ResourceNotFound:
                            message = "Verify that the provided Event Hubs Namespace contains the provided Event Hub and that the provided Event Hub contains the provided consumer group.";
                            break;
                        case EventHubsException.FailureReason.ServiceCommunicationProblem:
                            message = "Verify that the provided Event Hub Namespace, Event Hub name, and consumer group are correct and that access permissions to the provided Event Hub have been granted.";
                            break;
                        default:
                            return exception;
                    }

                    return new InvalidEventHubException(message, exception, nameof(EventHubErrorCode.ConfigurationError));

                case InvalidOperationException _:
                    message = "Verify that the provided Event Hub contains the provided consumer group.";
                    return new InvalidEventHubException(message, exception, nameof(EventHubErrorCode.ConfigurationError));

                case SocketException _:
                    SocketException socketException = (SocketException)exception;
                    switch (socketException.SocketErrorCode)
                    {
                        case SocketError.HostNotFound:
                            message = "Verify that the provided Event Hubs Namespace exists.";
                            return new InvalidEventHubException(message, exception, nameof(EventHubErrorCode.ConfigurationError));
                        default:
                            return exception;
                    }

                case UnauthorizedAccessException _:
                    message = "Verify that the provided Event Hub's 'Azure Event Hubs Data Receiver' role has been assigned to the applicable Azure Active Directory security principal or managed identity.";
                    string helpLink = "https://docs.microsoft.com/azure/event-hubs/authenticate-application";
                    return new UnauthorizedAccessEventHubException(message, exception, helpLink, nameof(EventHubErrorCode.AuthorizationError));

                default:
                    return exception;
            }
        }

        private static string GetErrorMetricName(Exception exception)
        {
            string errorName;

            switch (exception)
            {
                case EventHubsException _:
                    EventHubsException eventHubsException = (EventHubsException)exception;
                    errorName = eventHubsException.Reason.ToString();
                    break;
                case SocketException _:
                    SocketException socketException = (SocketException)exception;
                    errorName = socketException.SocketErrorCode.ToString();
                    break;
                default:
                    errorName = nameof(EventHubErrorCode.GeneralError);
                    break;
            }

            return $"{ErrorType.EventHubError}{errorName}";
        }
    }
}
