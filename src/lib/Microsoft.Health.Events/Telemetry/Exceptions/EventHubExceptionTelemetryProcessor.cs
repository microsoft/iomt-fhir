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
        public static void ProcessException(
            Exception exception,
            ITelemetryLogger logger,
            bool shouldLogMetric = true,
            string errorMetricName = null)
        {
            EnsureArg.IsNotNull(exception, nameof(exception));
            EnsureArg.IsNotNull(logger, nameof(logger));

            logger.LogError(exception);

            if (shouldLogMetric)
            {
                logger.LogMetric(EventMetrics.HandledException(errorMetricName ?? GetErrorMetricName(exception), ConnectorOperation.Setup), 1);
            }
        }

        private static string GetErrorMetricName(Exception exception)
        {
            switch (exception)
            {
                case EventHubsException _:
                    return $"{ErrorType.EventHubError}{EventHubErrorCode.OperationError}";
                case SocketException _:
                    return $"{ErrorType.EventHubError}{EventHubErrorCode.SocketError}";
                case UnauthorizedAccessException _:
                    return $"{ErrorType.EventHubError}{EventHubErrorCode.AuthorizationError}";
                default:
                    return $"{ErrorType.EventHubError}{EventHubErrorCode.GeneralError}";
            }
        }
    }
}
