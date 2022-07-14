// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Errors;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class FhirExceptionTelemetryProcessor : ExceptionTelemetryProcessor
    {
        private readonly string _connectorStage = ConnectorOperation.FHIRConversion;

        private IErrorMessageService _errorMessageService;

        public FhirExceptionTelemetryProcessor()
            : base (
                typeof(PatientDeviceMismatchException),
                typeof(ResourceIdentityNotDefinedException),
                typeof(NotSupportedException),
                typeof(FhirResourceNotFoundException),
                typeof(MultipleResourceFoundException<>),
                typeof(TemplateNotFoundException),
                typeof(CorrelationIdNotDefinedException),
                typeof(InvalidQuantityFhirValueException))
        {
        }

        // When an error message service is present, we do not need to retry certain exceptions indefinitely.
        // The following additional exceptions logged to the error message service immediately and are not retried:
        // - ValidationException (Template validation errors)
        // - MeasurementProcessingException (Could not convert event to measurement)
        // - FhirDataMappingException (FHIR mapping template exception)
        public FhirExceptionTelemetryProcessor(IErrorMessageService errorMessageService)
            : base(
                typeof(ValidationException),
                typeof(MeasurementProcessingException),
                typeof(FhirDataMappingException),
                typeof(PatientDeviceMismatchException),
                typeof(ResourceIdentityNotDefinedException),
                typeof(NotSupportedException),
                typeof(FhirResourceNotFoundException),
                typeof(MultipleResourceFoundException<>),
                typeof(TemplateNotFoundException),
                typeof(CorrelationIdNotDefinedException),
                typeof(InvalidQuantityFhirValueException))
                {
                    _errorMessageService = errorMessageService;
                }

        public override bool HandleException(Exception ex, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(logger, nameof(logger));

            var exceptionTypeName = ex.GetType().Name;

            var handledExceptionMetric = ex is NotSupportedException ? IomtMetrics.NotSupported() : IomtMetrics.HandledException(exceptionTypeName, _connectorStage);
            var handledException = HandleException(ex, logger, handledExceptionMetric, IomtMetrics.UnhandledException(exceptionTypeName, _connectorStage));

            if (handledException && _errorMessageService != null)
            {
                {
                    var errorMessage = new IomtErrorMessage(ex);
                    _errorMessageService.ReportError(errorMessage);
                }
            }

            return handledException;
        }
    }
}
