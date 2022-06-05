// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class NormalizationStreamingEventConsumerService : StreamingEventConsumerService
    {
        private readonly IContentTemplate _contentTemplate;
        private readonly IConverter<IEventMessage, JToken> _converter;
        private readonly IExceptionTelemetryProcessor _exceptionTelemetryProcessor;
        private readonly IEnumerableAsyncCollector<IMeasurement> _collector;

        public NormalizationStreamingEventConsumerService(
            EventBatchingOptions options,
            ICheckpointClient checkpointClient,
            ITelemetryLogger log,
            IEventProcessingMetricMeters eventProcessingMetricMeters,
            IConverter<IEventMessage, JToken> converter,
            IContentTemplate contentTemplate,
            IExceptionTelemetryProcessor exceptionTelemetryProcessor,
            IEnumerableAsyncCollector<IMeasurement> collector)
            : base(options, checkpointClient, log, eventProcessingMetricMeters)
        {
            _contentTemplate = EnsureArg.IsNotNull(contentTemplate, nameof(contentTemplate));
            _converter = EnsureArg.IsNotNull(converter, nameof(converter));
            _exceptionTelemetryProcessor = EnsureArg.IsNotNull(exceptionTelemetryProcessor, nameof(exceptionTelemetryProcessor));
            _collector = EnsureArg.IsNotNull(collector, nameof(collector));
        }

        protected override async Task ConsumeEventImpl(IEventMessage eventArg)
        {
            try
            {
                RecordLatencyMetrics(eventArg);

                var token = _converter.Convert(eventArg);

                await NormalizeMessage(token, eventArg);
            }
            catch (Exception ex)
            {
                if (!_exceptionTelemetryProcessor.HandleException(ex, Logger))
                {
                    throw; // Immediately throw originalexception if it is not handled
                }
            }
        }

        private async Task NormalizeMessage(JToken token, IEventMessage eventArg)
        {
            try
            {
                int projectedMeasurements = 0;
                foreach (var measurement in _contentTemplate.GetMeasurements(token))
                {
                    measurement.IngestionTimeUtc = eventArg.EnqueuedTime.UtcDateTime;
                    await _collector.AddAsync(measurement);
                    projectedMeasurements++;
                }

                RecordNormalizedEventMetrics(eventArg, projectedMeasurements);
            }
            catch (Exception ex)
            {
                // Translate all Normalization Mapping exceptions into a common type for easy identification.
                throw new NormalizationDataMappingException(ex);
            }
        }

        private void RecordLatencyMetrics(IEventMessage eventArg)
        {
            string partitionId = eventArg.PartitionId;
            TimeSpan deviceEventProcessingLatency = DateTime.UtcNow - eventArg.EnqueuedTime.UtcDateTime;

            // Fire and forget metric recording
            Task.Run(() =>
            {
                Logger.LogMetric(
                    IomtMetrics.DeviceEventProcessingLatency(partitionId),
                    deviceEventProcessingLatency.TotalSeconds);

                Logger.LogMetric(
                    IomtMetrics.DeviceEventProcessingLatencyMs(partitionId),
                    deviceEventProcessingLatency.TotalSeconds);
            });
        }

        private void RecordNormalizedEventMetrics(IEventMessage eventArg, int projectedMessages)
        {
            string partitionId = eventArg.PartitionId;

            // Fire and forget metric recording
            Task.Run(() =>
            {
                Logger.LogMetric(
                    IomtMetrics.NormalizedEvent(partitionId),
                    projectedMessages);
            });

            // TODO: Add new metric to track inbound to projected
        }
    }
}
