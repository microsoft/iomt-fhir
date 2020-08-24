// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class ExceptionTelemetryProcessor
    {
        private readonly HashSet<System.Type> _handledExceptions;

        public ExceptionTelemetryProcessor()
            : this (
                typeof(PatientDeviceMismatchException),
                typeof(ResourceIdentityNotDefinedException),
                typeof(NotSupportedException),
                typeof(FhirResourceNotFoundException),
                typeof(MultipleResourceFoundException<>),
                typeof(TemplateNotFoundException),
                typeof(CorrelationIdNotDefinedException))
        {
        }

        public ExceptionTelemetryProcessor(params System.Type[] handledExceptionTypes)
        {
            _handledExceptions = new HashSet<System.Type>(handledExceptionTypes);
        }

        public virtual bool HandleException(Exception ex, ITelemetryLogger log, string connectorStage)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(log, nameof(log));

            var exType = ex.GetType();

            var lookupType = exType.IsGenericType ? exType.GetGenericTypeDefinition() : exType;

            if (_handledExceptions.Contains(lookupType))
            {
                if (ex is ITelemetryFormattable tel)
                {
                    log.LogMetric(
                        metric: tel.ToMetric,
                        metricValue: 1);
                }
                else
                {
                    if (ex is NotSupportedException)
                    {
                        var metric = IomtMetrics.NotSupported();
                        log.LogMetric(
                            metric: metric,
                            metricValue: 1);
                    }
                    else
                    {
                        var metric = IomtMetrics.HandledException(
                            exType.Name,
                            connectorStage);
                        log.LogMetric(
                            metric: metric,
                            metricValue: 1);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
