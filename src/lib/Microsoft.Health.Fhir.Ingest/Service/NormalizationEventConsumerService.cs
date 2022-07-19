// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Errors;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;
using Polly;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class NormalizationEventConsumerService : IEventConsumer
    {
        private readonly IConverter<IEventMessage, JObject> _converter;
        private readonly string _templateDefinition;
        private readonly ITemplateManager _templateManager;
        private readonly ITelemetryLogger _logger;
        private readonly IEnumerableAsyncCollector<IMeasurement> _collector;
        private readonly AsyncPolicy _retryPolicy;
        private readonly CollectionTemplateFactory<IContentTemplate, IContentTemplate> _collectionTemplateFactory;
        private readonly IExceptionTelemetryProcessor _exceptionTelemetryProcessor;
        private readonly IErrorMessageService _errorMessageService;

        private readonly SemaphoreSlim semaphore = new (1);

        public NormalizationEventConsumerService(
            IConverter<IEventMessage, JObject> converter,
            string templateDefinition,
            ITemplateManager templateManager,
            IEnumerableAsyncCollector<IMeasurement> collector,
            ITelemetryLogger logger,
            CollectionTemplateFactory<IContentTemplate, IContentTemplate> collectionTemplateFactory,
            NormalizationExceptionTelemetryProcessor exceptionTelemetryProcessor,
            IErrorMessageService errorMessageService = null)
        {
            _converter = EnsureArg.IsNotNull(converter, nameof(converter));
            _templateDefinition = EnsureArg.IsNotNullOrWhiteSpace(templateDefinition, nameof(templateDefinition));
            _templateManager = EnsureArg.IsNotNull(templateManager, nameof(templateManager));
            _collector = EnsureArg.IsNotNull(collector, nameof(collector));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _retryPolicy = CreateRetryPolicy(logger);
            _collectionTemplateFactory = EnsureArg.IsNotNull(collectionTemplateFactory, nameof(collectionTemplateFactory));
            _exceptionTelemetryProcessor = EnsureArg.IsNotNull(exceptionTelemetryProcessor, nameof(exceptionTelemetryProcessor));
            _errorMessageService = errorMessageService;

            EventMetrics.SetConnectorOperation(ConnectorOperation.Normalization);
        }

        private (IContentTemplate template, DateTimeOffset timestamp) NormalizationTemplate { get; set; }

        public async Task ConsumeAsync(IEnumerable<IEventMessage> events)
        {
            EnsureArg.IsNotNull(events);

            var policyResult = await _retryPolicy.ExecuteAndCaptureAsync(async () => await ConsumeAsyncImpl(events));

            // This is a fallback option to skip any bad messages.
            // In known cases, the exception would be caught earlier and logged to the error message service.
            // If processing reaches this point in the code, the exception is unknown and retry attempts have failed.
            // If the error message service is enabled, the expectation is to log an error, and all events, and move on.
            if (_errorMessageService != null && policyResult.FinalException != null)
            {
                policyResult.FinalException.AddEventContext(events);
                var errorMessage = new IomtErrorMessage(policyResult.FinalException);
                _errorMessageService.ReportError(errorMessage);
            }
        }

        private async Task<IContentTemplate> GetNormalizationTemplate()
        {
            await semaphore.WaitAsync();
            try
            {
                using (ITimed templateGeneration = _logger.TrackDuration(IomtMetrics.NormalizationTemplateGenerationMs()))
                {
                    if (NormalizationTemplate.template == null)
                    {
                        _logger.LogTrace("Initializing normalization template from blob.");
                        DateTimeOffset timestamp = DateTimeOffset.UtcNow;
                        var content = await _templateManager.GetTemplateContentIfChangedSince(_templateDefinition);
                        var templateContext = _collectionTemplateFactory.Create(content);
                        templateContext.EnsureValid();
                        NormalizationTemplate = (templateContext.Template, timestamp);
                    }
                    else
                    {
                        DateTimeOffset updatedTimestamp = DateTimeOffset.UtcNow;
                        var content = await _templateManager.GetTemplateContentIfChangedSince(_templateDefinition, NormalizationTemplate.timestamp);

                        if (content != null)
                        {
                            _logger.LogTrace("New normalization template content detected, updating template.");
                            var templateContext = _collectionTemplateFactory.Create(content);
                            templateContext.EnsureValid();
                            NormalizationTemplate = (templateContext.Template, updatedTimestamp);
                        }
                    }

                    return NormalizationTemplate.template;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task ConsumeAsyncImpl(IEnumerable<IEventMessage> events)
        {
            // Step 1: Get the normalization mapping template
            IContentTemplate template = null;
            try
            {
                template = await GetNormalizationTemplate();
            }
            catch (Exception ex)
            {
                ex.AddEventContext(events);
                if (!_exceptionTelemetryProcessor.HandleException(ex, _logger))
                {
                    throw; // Immediately throw original exception if it is not handled
                }
                else
                {
                    return; // The exception was handled, so no need to proceed further when the template is invalid
                }
            }

            // Step 2: Normalize each event in the event batch
            var normalizationBatch = new List<(string sourcePartition, IMeasurement measurement)>(50);

            foreach (var evt in events)
            {
                try
                {
                    ProcessEvent(evt, template, normalizationBatch);
                }
                catch (Exception ex)
                {
                    ex.AddEventContext(evt);
                    if (!_exceptionTelemetryProcessor.HandleException(ex, _logger))
                    {
                        throw; // Immediately throw original exception if it is not handled
                    }
                }
            }

            // Step 3: Send normalized events to Event Hub
            try
            {
                _logger.LogMetric(IomtMetrics.MeasurementBatchSize(), normalizationBatch.Count);

                using (ITimed normalizeDuration = _logger.TrackDuration(IomtMetrics.MeasurementBatchSubmissionMs()))
                {
                    await _collector.AddAsync(items: normalizationBatch.Select(data => data.measurement), cancellationToken: CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                ex.AddEventContext(events);
                if (!_exceptionTelemetryProcessor.HandleException(ex, _logger))
                {
                    throw;
                }
            }

            // Step 4: Record Normalized Event Telemetry to correct partition
            foreach (var item in normalizationBatch)
            {
                _logger.LogMetric(IomtMetrics.NormalizedEvent(item.sourcePartition), 1);
            }
        }

        private void ProcessEvent(IEventMessage evt, IContentTemplate template, IList<(string sourcePartition, IMeasurement measurement)> collector)
        {
            RecordIngressMetrics(evt, _logger);

            var token = _converter.Convert(evt);

            NormalizeMessage(token: token, evt: evt, template: template, collector: collector);
        }

        private void NormalizeMessage(JObject token, IEventMessage evt, IContentTemplate template, IList<(string sourcePartition, IMeasurement measurement)> collector)
        {
            int projections = 0;

            using (ITimed normalizeDuration = _logger.TrackDuration(IomtMetrics.NormalizedEventGenerationTimeMs(evt.PartitionId)))
            {
                foreach (var measurement in template.GetMeasurements(token))
                {
                    try
                    {
                        measurement.IngestionTimeUtc = evt.EnqueuedTime.UtcDateTime;
                        collector.Add((evt.PartitionId, measurement));
                        projections++;
                    }
                    catch (Exception ex)
                    {
                        // Translate all Normalization Mapping exceptions into a common type for easy identification.
                        throw new NormalizationDataMappingException(ex, nameof(NormalizationDataMappingException))
                            .AddEventContext(evt);
                    }
                }
            }

            if (projections == 0)
            {
                _logger.LogTrace($"No measurements projected for event {evt.SequenceNumber}.");
                _logger.LogMetric(IomtMetrics.DroppedEvent(evt.PartitionId), 1);
            }
        }

        private static void RecordIngressMetrics(IEventMessage evt, ITelemetryLogger log)
        {
            TimeSpan deviceEventProcessingLatency = DateTime.UtcNow - evt.EnqueuedTime.UtcDateTime;

            log.LogMetric(
                IomtMetrics.DeviceEvent(evt.PartitionId),
                1);

            log.LogMetric(
                IomtMetrics.DeviceEventProcessingLatency(evt.PartitionId),
                deviceEventProcessingLatency.TotalSeconds);

            log.LogMetric(
                IomtMetrics.DeviceEventProcessingLatencyMs(evt.PartitionId),
                deviceEventProcessingLatency.TotalSeconds);
        }

        private AsyncPolicy CreateRetryPolicy(ITelemetryLogger logger)
        {
            bool ExceptionRetryableFilter(Exception ee)
            {
                logger.LogTrace($"Encountered retryable/unhandled exception {ee.GetType()}");
                logger.LogError(ee);
                TrackExceptionMetric(ee, logger);
                return true;
            }

            if (_errorMessageService != null)
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
            var type = exception.GetType().ToString();
            var metric = type.ToErrorMetric(ConnectorOperation.Normalization, ErrorType.DeviceMessageError, ErrorSeverity.Warning);
            logger.LogMetric(metric, 1);
        }
    }
}
