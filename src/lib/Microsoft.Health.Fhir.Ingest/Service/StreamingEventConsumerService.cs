// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public abstract class StreamingEventConsumerService : IEventConsumerService
    {
        protected const string ThresholdWaitReached = nameof(ThresholdWaitReached);
        protected const string ThresholdCountReached = nameof(ThresholdCountReached);
        protected const int PartitionCheckpointThreshold = 100;

        public StreamingEventConsumerService(EventBatchingOptions options, ICheckpointClient checkpointClient, ITelemetryLogger logger, IEventProcessingMetricMeters eventProcessingMetricMeters = null)
        {
            EventBatchingOptions = EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsInt(options.MaxEvents);
            EnsureArg.IsInt(options.FlushTimespan);

            CheckpointClient = EnsureArg.IsNotNull(checkpointClient, nameof(checkpointClient));
            Logger = EnsureArg.IsNotNull(logger, nameof(logger));
            EventProcessingMetrics = eventProcessingMetricMeters;

            PartitionProgress = new ConcurrentDictionary<string, EventPartitionProgressData>(8, 8);
        }

        protected ConcurrentDictionary<string, EventPartitionProgressData> PartitionProgress { get; }

        protected EventBatchingOptions EventBatchingOptions { get; }

        protected ICheckpointClient CheckpointClient { get; }

        protected ITelemetryLogger Logger { get; }

        protected IEventProcessingMetricMeters EventProcessingMetrics { get; }

        public async Task ConsumeEvent(IEventMessage eventArg)
        {
            if (eventArg is MaximumWaitEvent)
            {
                LogDataFreshness(DateTime.UtcNow, partitionId: eventArg.PartitionId, triggerReason: ThresholdWaitReached);
                return;
            }

            await ConsumeEventImpl(eventArg);

            await LogDeviceMetrics(eventArg);

            TrackPartitionProgress(eventArg);
        }

        public async Task ConsumeEvents(IEnumerable<IEventMessage> events)
        {
            foreach (var e in events)
            {
                await ConsumeEvent(e);
            }
        }

        protected abstract Task ConsumeEventImpl(IEventMessage eventArg);

        protected virtual async void TrackPartitionProgress(IEventMessage eventArg)
        {
            DateTimeOffset eventTime = eventArg.EnqueuedTime;

            var progress = PartitionProgress.AddOrUpdate(
                key: eventArg.PartitionId,
                addValueFactory: id => new EventPartitionProgressData { PartitionId = eventArg.PartitionId, MaxFreshness = eventArg.EnqueuedTime.UtcDateTime, EventsProcessed = 1 },
                updateValueFactory: (id, oldValue) =>
                {
                    oldValue.EventsProcessed = ++oldValue.EventsProcessed;
                    if (oldValue.MaxFreshness < eventTime)
                    {
                        oldValue.MaxFreshness = eventTime;
                    }

                    return oldValue;
                });

            if (progress.EventsProcessed % PartitionCheckpointThreshold == 0)
            {
                // Reached event threshold to update checkpoint and emit metrics
                await CheckpointClient.SetCheckpointAsync(eventArg);
                LogDataFreshness(eventTime.UtcDateTime, partitionId: eventArg.PartitionId, triggerReason: ThresholdCountReached);
            }
        }

        private void LogDataFreshness(DateTime freshness, string partitionId, string triggerReason)
        {
            // Immediately record Freshness, don't wait to continue processing.
            _ = Task.Run(() => Logger.LogMetric(EventMetrics.EventTimestampLastProcessedPerPartition(partitionId, triggerReason), double.Parse(freshness.ToString("yyyyMMddHHmmss"))));
        }

        public async Task LogDeviceMetrics(IEventMessage eventArg)
        {
            var metrics = await EventProcessingMetrics.GetMetrics(EventToEnumerable(eventArg));

            if (metrics == null)
            {
                return;
            }

            var deviceEvent = IomtMetrics.DeviceEvent(eventArg.PartitionId);

            // Immediately record metrics, don't wait to continue processing
            _ = Task.Run(() =>
            {
                Logger.LogMetric(metric: deviceEvent, metricValue: 1d);

                foreach (var metric in metrics)
                {
                    Logger.LogMetric(metric.Key, metric.Value);
                }
            });
        }

        private static IEnumerable<IEventMessage> EventToEnumerable(IEventMessage eventMessage)
        {
            yield return eventMessage;
        }

        protected struct EventPartitionProgressData
        {
            public string PartitionId { get; set; }

            public long EventsProcessed { get; set; }

            public DateTimeOffset MaxFreshness { get; set; }
        }
    }
}
