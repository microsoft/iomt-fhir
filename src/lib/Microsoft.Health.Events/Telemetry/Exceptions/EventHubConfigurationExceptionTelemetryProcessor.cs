// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.Telemetry.Exceptions
{
    public class EventHubConfigurationExceptionTelemetryProcessor
    {
        private readonly ISet<Type> _handledExceptions;

        public EventHubConfigurationExceptionTelemetryProcessor()
            : this(
                typeof(InvalidEventHubException),
                typeof(UnauthorizedAccessEventHubException))
        {
        }

        public EventHubConfigurationExceptionTelemetryProcessor(params Type[] handledExceptionTypes)
        {
            _handledExceptions = new HashSet<Type>(handledExceptionTypes);
        }

        public bool HandleException(Exception ex, ITelemetryLogger logger, string connectorStage)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(logger, nameof(logger));

            var exType = ex.GetType();

            var lookupType = exType.IsGenericType ? exType.GetGenericTypeDefinition() : exType;

            if (_handledExceptions.Contains(lookupType))
            {
                if (ex is ITelemetryFormattable tel)
                {
                    logger.LogMetric(
                        metric: tel.ToMetric,
                        metricValue: 1);
                }
                else
                {
                    var metric = EventMetrics.HandledException(
                        exType.Name,
                        connectorStage);
                    logger.LogMetric(
                        metric: metric,
                        metricValue: 1);
                }

                return true;
            }

            return false;
        }
    }
}
