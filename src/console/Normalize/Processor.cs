// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.EventHubs;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Fhir.Ingest.Console.Template;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Polly;
using static Microsoft.Azure.EventHubs.EventData;

namespace Microsoft.Health.Fhir.Ingest.Console.Normalize
{
    public class Processor : IEventConsumer
    {
        private string _templateDefinition;
        private ITemplateManager _templateManager;
        private ITelemetryLogger _logger;
        private IEnumerableAsyncCollector<IMeasurement> _collector;
        private AsyncPolicy _retryPolicy;
        private CollectionTemplateFactory<IContentTemplate, IContentTemplate> _collectionTemplateFactory;
        private IExceptionTelemetryProcessor _exceptionTelemetryProcessor;

        public Processor(
            string templateDefinition,
            ITemplateManager templateManager,
            IEnumerableAsyncCollector<IMeasurement> collector,
            ITelemetryLogger logger,
            CollectionTemplateFactory<IContentTemplate, IContentTemplate> collectionTemplateFactory)
        {
            _templateDefinition = EnsureArg.IsNotNullOrWhiteSpace(templateDefinition, nameof(templateDefinition));
            _templateManager = EnsureArg.IsNotNull(templateManager, nameof(templateManager));
            _collector = EnsureArg.IsNotNull(collector, nameof(collector));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _retryPolicy = CreateRetryPolicy(logger);
            _collectionTemplateFactory = EnsureArg.IsNotNull(collectionTemplateFactory, nameof(collectionTemplateFactory));
            _exceptionTelemetryProcessor = new NormalizationExceptionTelemetryProcessor();

            EventMetrics.SetConnectorOperation(ConnectorOperation.Normalization);
        }

        public async Task ConsumeAsync(IEnumerable<IEventMessage> events)
        {
            EnsureArg.IsNotNull(events);

            await _retryPolicy.ExecuteAsync(async () => await ConsumeAsyncImpl(events, _templateManager.GetTemplateAsString(_templateDefinition)));
        }

        private async Task ConsumeAsyncImpl(IEnumerable<IEventMessage> events, string templateContent)
        {
            var templateContext = _collectionTemplateFactory.Create(templateContent);
            templateContext.EnsureValid();
            var template = templateContext.Template;

            _logger.LogMetric(
                IomtMetrics.DeviceEvent(events.FirstOrDefault()?.PartitionId),
                    events.Count());

            IEnumerable<EventData> eventHubEvents = events
                .Select(x =>
                {
                    var eventData = new EventData(x.Body.ToArray());

                    eventData.SystemProperties = new SystemPropertiesCollection(
                        x.SequenceNumber,
                        x.EnqueuedTime.UtcDateTime,
                        x.Offset.ToString(),
                        x.PartitionId);

                    if (x.Properties != null)
                    {
                        foreach (KeyValuePair<string, object> entry in x.Properties)
                        {
                            eventData.Properties[entry.Key] = entry.Value;
                        }
                    }

                    if (x.SystemProperties != null)
                    {

                        foreach (KeyValuePair<string, object> entry in x.SystemProperties)
                        {
                            eventData.SystemProperties.TryAdd(entry.Key, entry.Value);
                        }
                    }

                    return eventData;
                });

            var dataNormalizationService = new MeasurementEventNormalizationService(_logger, template, _exceptionTelemetryProcessor);
            await dataNormalizationService.ProcessAsync(eventHubEvents, _collector).ConfigureAwait(false);
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
