// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
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
        private readonly IConverter<IEventMessage, JObject> _converter;
        private readonly IExceptionTelemetryProcessor _exceptionTelemetryProcessor;
        private readonly IEnumerableAsyncCollector<IMeasurement> _collector;

        public NormalizationStreamingEventConsumerService(
            EventBatchingOptions options,
            ICheckpointClient checkpointClient,
            ITelemetryLogger log,
            IEventProcessingMetricMeters eventProcessingMetricMeters,
            IConverter<IEventMessage, JObject> converter,
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

        /// <summary>
        /// Reusable list to collect projected measurements
        /// </summary>
        private List<IMeasurement> NormalizationBatch { get; } = new List<IMeasurement>(50);

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

        private async Task NormalizeMessage(JObject token, IEventMessage eventArg)
        {
            Stopwatch sw = Stopwatch.StartNew();
            foreach (var measurement in _contentTemplate.GetMeasurements(token))
            {
                try
                {
                    measurement.IngestionTimeUtc = eventArg.EnqueuedTime.UtcDateTime;
                    NormalizationBatch.Add(measurement);
                }
                catch (Exception ex)
                {
                    // Translate all Normalization Mapping exceptions into a common type for easy identification.
                    throw new NormalizationDataMappingException(ex);
                }
            }

            sw.Stop();

            // Send all projections at once so they can be batched and transismented efficiently.
            await _collector.AddAsync(NormalizationBatch);

            RecordNormalizedEventMetrics(eventArg, NormalizationBatch.Count, sw.Elapsed);

            // Clear projected measurements for next message.
            NormalizationBatch.Clear();
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

        private void RecordNormalizedEventMetrics(IEventMessage eventArg, int projectedMessages, TimeSpan duration)
        {
            string partitionId = eventArg.PartitionId;

            // Fire and forget metric recording
            Task.Run(() =>
            {
                Logger.LogMetric(
                    IomtMetrics.NormalizedEvent(partitionId),
                    projectedMessages);

                Logger.LogMetric(
                    IomtMetrics.NormalizedEventGenerationTimeMs(partitionId),
                    duration.TotalMilliseconds);

                if (projectedMessages == 0)
                {
                    Logger.LogTrace($"No measurements projected for event {eventArg.SequenceNumber}.");
                    Logger.LogMetric(
                        IomtMetrics.DroppedEvent(partitionId),
                        1);
                }
            });
        }
    }
}
