// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
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

        public NormalizationEventConsumerService(
            IConverter<IEventMessage, JObject> converter,
            string templateDefinition,
            ITemplateManager templateManager,
            IEnumerableAsyncCollector<IMeasurement> collector,
            ITelemetryLogger logger,
            CollectionTemplateFactory<IContentTemplate, IContentTemplate> collectionTemplateFactory,
            NormalizationExceptionTelemetryProcessor exceptionTelemetryProcessor)
        {
            _converter = EnsureArg.IsNotNull(converter, nameof(converter));
            _templateDefinition = EnsureArg.IsNotNullOrWhiteSpace(templateDefinition, nameof(templateDefinition));
            _templateManager = EnsureArg.IsNotNull(templateManager, nameof(templateManager));
            _collector = EnsureArg.IsNotNull(collector, nameof(collector));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _retryPolicy = CreateRetryPolicy(logger);
            _collectionTemplateFactory = EnsureArg.IsNotNull(collectionTemplateFactory, nameof(collectionTemplateFactory));
            _exceptionTelemetryProcessor = EnsureArg.IsNotNull(exceptionTelemetryProcessor, nameof(exceptionTelemetryProcessor));

            EventMetrics.SetConnectorOperation(ConnectorOperation.Normalization);
        }

        public async Task ConsumeAsync(IEnumerable<IEventMessage> events)
        {
            EnsureArg.IsNotNull(events);

            await _retryPolicy.ExecuteAsync(async () => await ConsumeAsyncImpl(events));
        }

        private Task<IContentTemplate> GetNormalizationTemplate()
        {
            var content = _templateManager.GetTemplateAsString(_templateDefinition);
            var templateContext = _collectionTemplateFactory.Create(content);
            templateContext.EnsureValid();
            return Task.FromResult(templateContext.Template);
        }

        private async Task ConsumeAsyncImpl(IEnumerable<IEventMessage> events)
        {
            var template = await GetNormalizationTemplate();

            var normalizationBatch = new List<(string sourcePartition, IMeasurement measurement)>(50);

            foreach (var evt in events)
            {
                ProcessEvent(evt, template, normalizationBatch);
            }

            // Send normalized events
            await _collector.AddAsync(items: normalizationBatch.Select(data => data.measurement), cancellationToken: CancellationToken.None);

            // Record Normalized Event Telemetry to correct partition
            foreach (var item in normalizationBatch)
            {
                _logger.LogMetric(IomtMetrics.NormalizedEvent(item.sourcePartition), 1);
            }
        }

        private void ProcessEvent(IEventMessage evt, IContentTemplate template, IList<(string sourcePartition, IMeasurement measurement)> collector)
        {
            try
            {
                RecordIngressMetrics(evt, _logger);

                var token = _converter.Convert(evt);

                NormalizeMessage(token: token, evt: evt, template: template, collector: collector);
            }
            catch (Exception ex)
            {
                if (!_exceptionTelemetryProcessor.HandleException(ex, _logger))
                {
                    throw; // Immediately throw original exception if it is not handled
                }
            }
        }

        private void NormalizeMessage(JObject token, IEventMessage evt, IContentTemplate template, IList<(string sourcePartition, IMeasurement measurement)> collector)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int projections = 0;

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
                    throw new NormalizationDataMappingException(ex);
                }
            }

            sw.Stop();

            RecordNormalizationEventMetrics(evt, projections, sw.Elapsed, _logger);
        }

        private static void RecordIngressMetrics(IEventMessage evt, ITelemetryLogger log)
        {
            TimeSpan deviceEventProcessingLatency = DateTime.UtcNow - evt.EnqueuedTime.UtcDateTime;

            log.LogMetric(
                IomtMetrics.DeviceEventProcessingLatency(evt.PartitionId),
                deviceEventProcessingLatency.TotalSeconds);

            log.LogMetric(
                IomtMetrics.DeviceEventProcessingLatencyMs(evt.PartitionId),
                deviceEventProcessingLatency.TotalSeconds);
        }

        private static void RecordNormalizationEventMetrics(IEventMessage evt, int projectedMessages, TimeSpan duration, ITelemetryLogger log)
        {
            log.LogMetric(IomtMetrics.NormalizedEventGenerationTimeMs(evt.PartitionId), duration.TotalMilliseconds);

            if (projectedMessages == 0)
            {
                log.LogTrace($"No measurements projected for event {evt.SequenceNumber}.");
                log.LogMetric(IomtMetrics.DroppedEvent(evt.PartitionId), 1);
            }
        }

        private static AsyncPolicy CreateRetryPolicy(ITelemetryLogger logger)
        {
            // Retry on any unhandled exceptions.
            // TODO (WI - 86288): Handled exceptions (eg: data errors) will not be retried upon indefinitely.
            bool ExceptionRetryableFilter(Exception ee)
            {
                logger.LogTrace($"Encountered retryable/unhandled exception {ee.GetType()}");
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
            var metric = type.ToErrorMetric(ConnectorOperation.Normalization, ErrorType.DeviceMessageError, ErrorSeverity.Warning);
            logger.LogMetric(metric, 1);
        }
    }
}
