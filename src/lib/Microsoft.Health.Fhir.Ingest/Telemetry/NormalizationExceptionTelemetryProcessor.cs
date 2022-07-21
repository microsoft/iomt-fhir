// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Errors;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class NormalizationExceptionTelemetryProcessor : ExceptionTelemetryProcessor
    {
        private readonly string _connectorStage = ConnectorOperation.Normalization;

        private static readonly Type[] DefaultExceptions = new[]
        {
            typeof(IncompatibleDataException),
            typeof(InvalidDataFormatException),
        };

        private IErrorMessageService _errorMessageService;
        private static readonly Type[] DefaultExceptionsWithErrorMessageService = new[]
        {
            typeof(IncompatibleDataException),
            typeof(InvalidDataFormatException),
            typeof(ValidationException),
            typeof(NormalizationDataMappingException),
            typeof(EventHubProducerClientException),
        };

        public NormalizationExceptionTelemetryProcessor(IExceptionTelemetryProcessorConfig exceptionConfig)
        : base(exceptionConfig.HandledExceptionTypes.Union(DefaultExceptions).ToArray())
        {
        }

        public NormalizationExceptionTelemetryProcessor(IExceptionTelemetryProcessorConfig exceptionConfig, IErrorMessageService errorMessageService)
            : base(exceptionConfig.HandledExceptionTypes.Union(DefaultExceptionsWithErrorMessageService).ToArray())
        {
            _errorMessageService = errorMessageService;
        }

        public override bool HandleException(Exception ex, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(logger, nameof(logger));

            var exceptionTypeName = ex.GetType().Name;
            var handledException = HandleException(ex, logger, IomtMetrics.HandledException(exceptionTypeName, _connectorStage));

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
