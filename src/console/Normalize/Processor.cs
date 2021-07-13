// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using AzureMessagingEventHubs = Azure.Messaging.EventHubs;
using Azure.Identity;
using EnsureThat;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.Model;
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
        private IAsyncCollector<IMeasurement> _collector;
        private IOptions<NormalizationServiceOptions> _normalizationOptions;
        private IEventProcessingMeter _eventProcessingMeter = new EventProcessingMeter();
        private AsyncPolicy _retryPolicy;

        public Processor(
            [Blob("template/%Template:DeviceContent%", FileAccess.Read)] string templateDefinition,
            ITemplateManager templateManager,
            IAsyncCollector<IMeasurement> collector,
            ITelemetryLogger logger,
            IOptions<NormalizationServiceOptions> options)
        {
            _templateDefinition = EnsureArg.IsNotNullOrWhiteSpace(templateDefinition, nameof(templateDefinition));
            _templateManager = EnsureArg.IsNotNull(templateManager, nameof(templateManager));
            _collector = EnsureArg.IsNotNull(collector, nameof(collector));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _normalizationOptions = EnsureArg.IsNotNull(options, nameof(options));
            _retryPolicy = CreateRetryPolicy(logger);
        }

        public async Task ConsumeAsync(IEnumerable<IEventMessage> events)
        {
            EnsureArg.IsNotNull(events);

            await _retryPolicy.ExecuteAsync(async () => await ConsumeAsyncImpl(events, _templateManager.GetTemplateAsString(_templateDefinition)));
        }

        private async Task ConsumeAsyncImpl(IEnumerable<IEventMessage> events, string templateContent)
        {
            var templateContext = CollectionContentTemplateFactory.Default.Create(templateContent);
            templateContext.EnsureValid();
            var template = templateContext.Template;

            _logger.LogMetric(
                IomtMetrics.DeviceEvent(),
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

                    foreach (KeyValuePair<string, object> entry in x.Properties)
                    {
                        eventData.Properties[entry.Key] = entry.Value;
                    }

                    foreach (KeyValuePair<string, object> entry in x.SystemProperties)
                    {
                        eventData.SystemProperties.TryAdd(entry.Key, entry.Value);
                    }

                    return eventData;
                });

            var dataNormalizationService = new MeasurementEventNormalizationService(_logger, template);
            await dataNormalizationService.ProcessAsync(eventHubEvents, _collector).ConfigureAwait(false);

            if (_normalizationOptions.Value.LogDeviceIngressSizeBytes)
            {
                var eventStats = await _eventProcessingMeter.CalculateEventStats(eventHubEvents);

                _logger.LogMetric(
                    IomtMetrics.DeviceIngressSizeBytes(),
                    eventStats.TotalEventsProcessedBytes);
            }
        }

        private static AsyncPolicy CreateRetryPolicy(ITelemetryLogger logger)
        {
            bool ExceptionRetryableFilter(Exception ee)
            {
                switch (ee)
                {
                    case AggregateException ae when ae.InnerExceptions.Any(ExceptionRetryableFilter):
                    case OperationCanceledException _:
                    case HttpRequestException _:
                    case AzureMessagingEventHubs.EventHubsException _:
                    case AuthenticationFailedException _:
                    case RequestFailedException _:
                        break;
                    default:
                        TrackExceptionMetric(ee, logger);
                        return false;
                }

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
            var ToMetric = new Metric(
                type,
                new Dictionary<string, object>
                {
                    { DimensionNames.Name, type },
                    { DimensionNames.Category, Category.Errors },
                    { DimensionNames.ErrorType, ErrorType.DeviceMessageError },
                    { DimensionNames.ErrorSeverity, ErrorSeverity.Warning },
                    { DimensionNames.Operation, ConnectorOperation.Normalization},
                });
            logger.LogMetric(ToMetric, 1);
        }
    }
}
