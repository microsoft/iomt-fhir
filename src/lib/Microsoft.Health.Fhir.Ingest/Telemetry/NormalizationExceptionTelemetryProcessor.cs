// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Common.Errors;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class NormalizationExceptionTelemetryProcessor : ExceptionTelemetryProcessor
    {
        private readonly string _connectorStage = ConnectorOperation.Normalization;
        private static readonly Type[] DefaultExceptions = new[] { typeof(IncompatibleDataException) };
        private IErrorMessageService _errorMessageService;

        public NormalizationExceptionTelemetryProcessor(params Type[] handledExceptionTypes)
            : base(handledExceptionTypes.Union(DefaultExceptions).ToArray())
        {
        }

        public NormalizationExceptionTelemetryProcessor(IErrorMessageService errorMessageService, params Type[] handledExceptionTypes)
            : base(handledExceptionTypes.Union(DefaultExceptions).ToArray())
        {
            _errorMessageService = errorMessageService;
        }

        public override bool HandleException(Exception ex, JToken message, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(logger, nameof(logger));

            var exceptionTypeName = ex.GetType().Name;
            var shouldContinue = HandleException(ex, logger, IomtMetrics.HandledException(exceptionTypeName, _connectorStage), IomtMetrics.UnhandledException(exceptionTypeName, _connectorStage));

            if (_errorMessageService != null)
            {
                var errorMessage = new ErrorMessage();
                errorMessage.InputMessage = message;
                errorMessage.Details = ex.Message;
                errorMessage.Type = exceptionTypeName;
                _errorMessageService.ReportError(errorMessage, default);
            }

            return shouldContinue;
        }
    }
}
