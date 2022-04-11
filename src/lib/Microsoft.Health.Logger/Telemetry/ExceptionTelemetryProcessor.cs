// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using System;
using System.Collections.Generic;

namespace Microsoft.Health.Logging.Telemetry
{
    public class ExceptionTelemetryProcessor : IExceptionTelemetryProcessor
    {
        private readonly ISet<Type> _handledExceptions;

        public ExceptionTelemetryProcessor()
        {
            _handledExceptions = new HashSet<Type>();
        }

        public ExceptionTelemetryProcessor(params Type[] handledExceptionTypes)
        {
            _handledExceptions = new HashSet<Type>(handledExceptionTypes);
        }

        public virtual bool HandleException(
            Exception ex,
            ITelemetryLogger logger)
        {
            return HandleException(ex, logger);
        }

        public virtual void LogExceptionMetric(
            Exception ex,
            ITelemetryLogger logger,
            Metric exceptionMetric = null)
        {
            Metric metric = ex is ITelemetryFormattable tel ? tel.ToMetric : exceptionMetric;
            if (metric != null)
            {
                logger.LogMetric(metric, metricValue: 1);
            }
        }

        /// <summary>
        /// Evaluates if the exception is handleable, i.e., can be continued upon.
        /// The associated exception metric is logged.
        /// </summary>
        /// <param name="ex">Exception that is to be evaluated as handleable or not.</param>
        /// <param name="logger">Telemetry logger used to log the exception/metric.</param>
        /// <param name="handledExceptionMetric">Exception metric that is to be logged if the passed in exception is handled and not of type ITelemetryFormattable.</param>
        /// <param name="unhandledExceptionMetric">Exception metric that is to be logged if the passed in exception is unhandled and not of type ITelemetryFormattable.</param>
        /// <returns>Returns true if the exception is handleable, i.e., can be continued upon. False otherwise.</returns>
        protected bool HandleException(
            Exception ex,
            ITelemetryLogger logger,
            Metric handledExceptionMetric = null,
            Metric unhandledExceptionMetric = null)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(logger, nameof(logger));

            var exType = ex.GetType();
            var lookupType = exType.IsGenericType ? exType.GetGenericTypeDefinition() : exType;

            if (_handledExceptions.Contains(lookupType))
            {
                LogExceptionMetric(ex, logger, handledExceptionMetric);
                return true;
            }

            logger.LogError(ex);
            LogExceptionMetric(ex, logger, unhandledExceptionMetric);

            return false;
        }
    }
}
