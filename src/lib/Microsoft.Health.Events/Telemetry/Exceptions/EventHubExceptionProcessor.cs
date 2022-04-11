// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net.Sockets;
using Azure;
using Azure.Messaging.EventHubs;
using EnsureThat;
using Microsoft.Health.Common.Extension;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;
using Microsoft.Health.Events.Resources;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Identity.Client;

namespace Microsoft.Health.Events.Telemetry.Exceptions
{
    public static class EventHubExceptionProcessor
    {
        private static readonly IExceptionTelemetryProcessor _exceptionTelemetryProcessor = new ExceptionTelemetryProcessor();

        public static void ProcessException(
            Exception exception,
            ITelemetryLogger logger,
            string errorMetricName = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            var (customException, errorName) = CustomizeException(exception);

            logger.LogError(customException);

            errorMetricName = customException.Equals(exception) ? errorMetricName ?? $"{ErrorType.EventHubError}{errorName}" : customException.GetType().Name;
            _exceptionTelemetryProcessor.LogExceptionMetric(customException, logger, EventMetrics.HandledException(errorMetricName, ConnectorOperation.Setup));
        }

        public static (Exception customException, string errorName) CustomizeException(Exception exception)
        {
            EnsureArg.IsNotNull(exception, nameof(exception));

            string message = exception.Message;
            string errorName;

            switch (exception)
            {
                case EventHubsException _:
                    var reason = ((EventHubsException)exception).Reason;
                    switch (reason)
                    {
                        case EventHubsException.FailureReason.ResourceNotFound:
                            message = EventResources.EventHubResourceNotFound;
                            break;
                        case EventHubsException.FailureReason.ServiceCommunicationProblem:
                            message = EventResources.EventHubServiceCommunicationProblem;
                            break;
                        default:
                            return (exception, reason.ToString());
                    }

                    errorName = nameof(EventHubErrorCode.ConfigurationError);
                    return (new InvalidEventHubException(message, exception, errorName), errorName);

                case InvalidOperationException _:
                    if (message.Contains(EventResources.ConsumerGroup, StringComparison.CurrentCultureIgnoreCase))
                    {
                        message = EventResources.EventHubInvalidConsumerGroup;
                        errorName = nameof(EventHubErrorCode.ConfigurationError);
                        return (new InvalidEventHubException(message, exception, errorName), errorName);
                    }

                    return (exception, nameof(EventHubErrorCode.InvalidOperationError));

                case MsalServiceException _:
                    var msalErrorCode = ((MsalServiceException)exception).ErrorCode;
                    message = EventResources.ManagedIdentityAuthenticationError;
                    return (new ManagedIdentityAuthenticationError(message, exception, msalErrorCode), msalErrorCode);

                case RequestFailedException _:
                    var errorCode = ((RequestFailedException)exception).ErrorCode;

                    if (string.Equals(errorCode, EventResources.SecretNotFound, StringComparison.CurrentCultureIgnoreCase) || message.Contains(EventResources.SecretNotFound, StringComparison.CurrentCultureIgnoreCase))
                    {
                        message = EventResources.ManagedIdentityCredentialNotFound;
                        return (new ManagedIdentityCredentialNotFound(message, exception), errorCode);
                    }

                    return (exception, $"{EventHubErrorCode.RequestFailedError}{errorCode}");

                case SocketException _:
                    var socketErrorCode = ((SocketException)exception).SocketErrorCode;
                    switch (socketErrorCode)
                    {
                        case SocketError.HostNotFound:
                            message = EventResources.EventHubHostNotFound;
                            errorName = nameof(EventHubErrorCode.ConfigurationError);
                            return (new InvalidEventHubException(message, exception, errorName), errorName);
                        default:
                            return (exception, socketErrorCode.ToString());
                    }

                case UnauthorizedAccessException _:
                    message = EventResources.EventHubAuthorizationError;
                    string helpLink = "https://docs.microsoft.com/azure/event-hubs/authenticate-application";
                    errorName = nameof(EventHubErrorCode.AuthorizationError);
                    return (new UnauthorizedAccessEventHubException(message, exception, helpLink, errorName), errorName);

                default:
                    return (exception, nameof(EventHubErrorCode.GeneralError));
            }
        }
    }
}
