// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Fhir.Ingest.Service;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class ExceptionTelemetryProcessor
    {
        private readonly HashSet<Type> _handledExceptions;

        public ExceptionTelemetryProcessor()
            : this (
                typeof(PatientDeviceMismatchException),
                typeof(ResourceIdentityNotDefinedException),
                typeof(NotSupportedException),
                typeof(FhirResourceNotFoundException),
                typeof(MultipleResourceFoundException<>))
        {
        }

        public ExceptionTelemetryProcessor(params Type[] handledExceptionTypes)
        {
            _handledExceptions = new HashSet<Type>(handledExceptionTypes);
        }

        public virtual bool HandleException(Exception ex, ILogger log)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            var exType = ex.GetType();

            var lookupType = exType.IsGenericType ? exType.GetGenericTypeDefinition() : exType;

            if (_handledExceptions.Contains(lookupType))
            {
                if (ex is ITelemetryEvent evt)
                {
                    log.LogMetric(name: evt.EventName, value: 1);
                }
                else
                {
                    log.LogMetric(name: exType.Name, value: 1);
                }

                return true;
            }

            return false;
        }
    }
}
