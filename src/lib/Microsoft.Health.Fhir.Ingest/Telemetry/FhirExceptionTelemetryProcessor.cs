// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
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

        public override bool HandleException(Exception ex, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(logger, nameof(logger));

            var exceptionTypeName = ex.GetType().Name;
            var handledExceptionMetric = ex is NotSupportedException ? IomtMetrics.NotSupported() : IomtMetrics.HandledException(exceptionTypeName, _connectorStage);
            return HandleException(ex, logger, handledExceptionMetric, IomtMetrics.UnhandledException(exceptionTypeName, _connectorStage));
        }
    }
}
