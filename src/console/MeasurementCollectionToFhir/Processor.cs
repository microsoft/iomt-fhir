// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Fhir.Ingest.Console.Template;
using Microsoft.Health.Fhir.Ingest.Host;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Logging.Telemetry;
using Polly;

namespace Microsoft.Health.Fhir.Ingest.Console.MeasurementCollectionToFhir
{
    public class Processor : IEventConsumer
    {
        private ITemplateManager _templateManager;
        private MeasurementFhirImportService _measurementImportService;
        private string _templateDefinition;
        private ITelemetryLogger _logger;
        private AsyncPolicy _retryPolicy;

        public Processor(
            [Blob("template/%Template:FhirMapping%", FileAccess.Read)] string templateDefinition,
            ITemplateManager templateManager,
            [MeasurementFhirImport] MeasurementFhirImportService measurementImportService,
            ITelemetryLogger logger)
        {
            _templateDefinition = EnsureArg.IsNotNullOrWhiteSpace(templateDefinition, nameof(templateDefinition));
            _templateManager = EnsureArg.IsNotNull(templateManager, nameof(templateManager));
            _measurementImportService = EnsureArg.IsNotNull(measurementImportService, nameof(measurementImportService));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _retryPolicy = CreateRetryPolicy(logger);

            EventMetrics.SetConnectorOperation(ConnectorOperation.FHIRConversion);
        }

        public async Task ConsumeAsync(IEnumerable<IEventMessage> events)
        {
            EnsureArg.IsNotNull(events);

            await _retryPolicy.ExecuteAsync(async () => await ConsumeAsyncImpl(events, _templateManager.GetTemplateAsString(_templateDefinition)));
        }

        private async Task ConsumeAsyncImpl(IEnumerable<IEventMessage> events, string templateContent)
        {
            await _measurementImportService.ProcessEventsAsync(events, templateContent, _logger).ConfigureAwait(false);
        }

        private static AsyncPolicy CreateRetryPolicy(ITelemetryLogger logger)
        {
            bool ExceptionRetryableFilter(Exception ee)
            {
                logger.LogTrace($"Encountered retryable exception {ee.GetType()}");
                logger.LogError(ee);
                TrackExceptionMetric(ee, logger);
                return true;
            }

            return Policy
                .Handle<Exception>(ExceptionRetryableFilter)
                .WaitAndRetryForeverAsync(retryCount => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, retryCount))));
        }

        private static void TrackExceptionMetric(Exception exception, ITelemetryLogger logger)
        {
            var type = exception.GetType().ToString();
            var metric = type.ToErrorMetric(ConnectorOperation.FHIRConversion, ErrorType.GeneralError, ErrorSeverity.Warning);
            logger.LogMetric(metric, 1);
        }
    }
}
