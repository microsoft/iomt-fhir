﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Errors;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Polly;

namespace Microsoft.Health.Fhir.Ingest.Console.FhirTransformation
{
    public class Processor : IEventConsumer
    {
        private ITemplateManager _templateManager;
        private IImportService _measurementImportService;
        private string _templateName;
        private ITelemetryLogger _logger;
        private IErrorMessageService? _errorMessageService;
        private AsyncPolicy _retryPolicy;

        public Processor(
            IOptions<TemplateOptions> templateOptions,
            ITemplateManager templateManager,
            IImportService measurementImportService,
            ITelemetryLogger logger,
            IErrorMessageService? errorMessageService = null)
        {
            EnsureArg.IsNotNull(templateOptions, nameof(templateOptions));
            _templateName = EnsureArg.IsNotNullOrWhiteSpace(templateOptions.Value.FhirMapping, nameof(templateOptions.Value.FhirMapping));
            _templateManager = EnsureArg.IsNotNull(templateManager, nameof(templateManager));
            _measurementImportService = EnsureArg.IsNotNull(measurementImportService, nameof(measurementImportService));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _errorMessageService = errorMessageService;
            _retryPolicy = CreateRetryPolicy(logger, errorMessageService);

            EventMetrics.SetConnectorOperation(ConnectorOperation.FHIRConversion);
        }

        public async Task ConsumeAsync(IEnumerable<IEventMessage> events, CancellationToken ct)
        {
            EnsureArg.IsNotNull(events);

            var policyResult = await _retryPolicy.ExecuteAndCaptureAsync(async () => await ConsumeAsyncImpl(events, _templateManager.GetTemplateAsString(_templateName), ct));

            // This is a fallback option to skip any bad messages.
            // In known cases, the exception would be caught earlier and logged to the error message service.
            // If processing reaches this point in the code, the exception is unknown and retry attempts have failed.
            // If the error message service is enabled, the expectation is to log an error, and all events, and move on.
            if (_errorMessageService != null
                && policyResult.FinalException != null
                && policyResult.FinalException is not TaskCanceledException)
            {
                policyResult.FinalException.AddEventContext(events);
                var errorMessage = new IomtErrorMessage(policyResult.FinalException);
                _errorMessageService?.ReportError(errorMessage);
            }

        }

        private async Task ConsumeAsyncImpl(IEnumerable<IEventMessage> events, string templateContent, CancellationToken ct)
        {
            await _measurementImportService.ProcessEventsAsync(events, templateContent, _logger, ct).ConfigureAwait(false);
        }

        private static AsyncPolicy CreateRetryPolicy(ITelemetryLogger logger, IErrorMessageService? errorMessageService)
        {
            bool ExceptionRetryableFilter(Exception ee)
            {
                if (ee is TaskCanceledException)
                {
                    logger.LogTrace($"The task was cancelled.");
                    return false;
                }

                logger.LogTrace($"Encountered retryable exception {ee.GetType()}");
                logger.LogError(ee);
                TrackExceptionMetric(ee, logger);
                return true;
            }

            // if error message service enabled then only retry 10 times with exp backoff
            if (errorMessageService != null)
            {
                return Policy
                    .Handle<Exception>(ExceptionRetryableFilter)
                    .WaitAndRetryAsync(retryCount: 10, sleepDurationProvider: (retryCount) => TimeSpan.FromSeconds(Math.Min(15, Math.Pow(2, retryCount))));
            }

            return Policy
                .Handle<Exception>(ExceptionRetryableFilter)
                .WaitAndRetryForeverAsync(retryCount => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, retryCount))));
        }

        private static void TrackExceptionMetric(Exception exception, ITelemetryLogger logger)
        {
            var metric = IomtMetrics.UnhandledException(exception.GetType().ToString(), ConnectorOperation.FHIRConversion, ErrorType.GeneralError, ErrorSeverity.Warning);
            logger.LogMetric(metric, 1);
        }
    }
}
