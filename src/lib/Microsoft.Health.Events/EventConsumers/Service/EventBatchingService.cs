// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service.Infrastructure;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventConsumers.Service
{
    public class EventBatchingService : IEventConsumerService
    {
        private ConcurrentDictionary<string, EventPartition> _eventPartitions;
        private int _maxEvents;
        private TimeSpan _flushTimespan;
        private IEventConsumerService _eventConsumerService;
        private IEventProcessingMetricMeters _eventProcessingMetricMeters;
        private ICheckpointClient _checkpointClient;
        private ITelemetryLogger _logger;
        private const int _timeBuffer = -5;

        public EventBatchingService(IEventConsumerService eventConsumerService, EventBatchingOptions options, ICheckpointClient checkpointClient, ITelemetryLogger logger, IEventProcessingMetricMeters eventProcessingMetricMeters = null)
        {
            EnsureArg.IsNotNull(options);
            EnsureArg.IsInt(options.MaxEvents);
            EnsureArg.IsInt(options.FlushTimespan);

            _eventPartitions = new ConcurrentDictionary<string, EventPartition>();
            _eventConsumerService = eventConsumerService;
            _eventProcessingMetricMeters = eventProcessingMetricMeters;
            _maxEvents = options.MaxEvents;
            _flushTimespan = TimeSpan.FromSeconds(options.FlushTimespan);
            _checkpointClient = checkpointClient;
            _logger = logger;
        }

        public EventPartition GetPartition(string partitionId)
        {
            EnsureArg.IsNotNullOrWhiteSpace(partitionId);

            if (!_eventPartitions.ContainsKey(partitionId))
            {
                throw new Exception($"Partition with identifier {partitionId} does not exist");
            }

            return _eventPartitions[partitionId];
        }

        private bool EventPartitionExists(string partitionId)
        {
            return _eventPartitions.ContainsKey(partitionId);
        }

        private EventPartition CreatePartitionIfMissing(string partitionId, DateTime initTime, TimeSpan flushTimespan)
        {
            return _eventPartitions.GetOrAdd(partitionId, new EventPartition(partitionId, initTime, flushTimespan, _logger));
        }

        public async Task ConsumeEvent(IEventMessage eventArg)
        {
            EnsureArg.IsNotNull(eventArg);

            var partitionId = eventArg.PartitionId;
            var eventEnqueuedTime = eventArg.EnqueuedTime.UtcDateTime;

            if (eventArg is MaximumWaitEvent)
            {
                if (EventPartitionExists(partitionId))
                {
                    var windowThresholdTime = GetPartition(partitionId).GetPartitionWindow();
                    await ThresholdWaitReached(partitionId, windowThresholdTime);
                }
                else
                {
                    // If we received the timer event and there are no enqueued events in the partition, then simply update the data freshness.
                    LogDataFreshness(partitionId, triggerReason: nameof(ThresholdWaitReached));
                }
            }
            else
            {
                var partition = CreatePartitionIfMissing(partitionId, eventEnqueuedTime, _flushTimespan);

                partition.Enqueue(eventArg);

                var windowThresholdTime = partition.GetPartitionWindow();
                if (eventEnqueuedTime > windowThresholdTime)
                {
                    await ThresholdTimeReached(partitionId, eventArg, windowThresholdTime);
                }

                if (partition.GetPartitionBatchCount() >= _maxEvents)
                {
                    await ThresholdCountReached(partitionId);
                }
            }
        }

        private async Task ThresholdCountReached(string partitionId)
        {
            _logger.LogTrace($"Partition {partitionId} threshold count {_maxEvents} was reached.");
            var events = await GetPartition(partitionId).Flush(_maxEvents);
            await CompleteProcessing(partitionId, events, triggerReason: nameof(ThresholdCountReached));
        }

        private async Task ThresholdTimeReached(string partitionId, IEventMessage eventArg, DateTime windowEnd)
        {
            _logger.LogTrace($"Partition {partitionId} threshold time {_eventPartitions[partitionId].GetPartitionWindow()} was reached.");

            var queue = GetPartition(partitionId);
            var events = await queue.Flush(windowEnd);
            queue.IncrementPartitionWindow(eventArg.EnqueuedTime.UtcDateTime);
            await CompleteProcessing(partitionId, events, triggerReason: nameof(ThresholdTimeReached));
        }

        private async Task ThresholdWaitReached(string partitionId, DateTime windowEnd)
        {
            if (windowEnd < DateTime.UtcNow.AddSeconds(_timeBuffer))
            {
                _logger.LogTrace($"Partition {partitionId} threshold wait reached. Flushing {_eventPartitions[partitionId].GetPartitionBatchCount()} events up to: {windowEnd}");
                var events = await GetPartition(partitionId).Flush(windowEnd);
                await CompleteProcessing(partitionId, events, triggerReason: nameof(ThresholdWaitReached));
            }
        }

        private async Task UpdateCheckpoint(IEnumerable<IEventMessage> events, IEnumerable<KeyValuePair<Metric, double>> eventMetrics)
        {
            if (events.Count() > 0)
            {
                var eventCheckpoint = events.ElementAt(events.Count() - 1);
                await _checkpointClient.SetCheckpointAsync(eventCheckpoint, eventMetrics);
            }
        }

        private async Task CompleteProcessing(string partitionId, IEnumerable<IEventMessage> events, string triggerReason)
        {
            await _eventConsumerService.ConsumeEvents(events);

            IEnumerable<KeyValuePair<Metric, double>> eventMetrics = null;

            if (_eventProcessingMetricMeters != null)
            {
                eventMetrics = await _eventProcessingMetricMeters.GetMetrics(events);
            }

            await UpdateCheckpoint(events, eventMetrics);

            LogDataFreshness(partitionId, triggerReason, events);
        }

        public Task ConsumeEvents(IEnumerable<IEventMessage> events)
        {
            throw new NotImplementedException();
        }

        private void LogDataFreshness(string partitionId, string triggerReason, IEnumerable<IEventMessage> events = null)
        {
            // To determine the data freshness per partition (i.e. latest event data processed in a partition), use the enqueued time of the last event for the batch.
            // If no events were flushed for the partition (eg: trigger reason is ThresholdWaitReached - due to receival of MaxTimeEvent), then use the current timestamp.
            var eventTimestampLastProcessed = events?.Any() ?? false ? events.Last().EnqueuedTime.UtcDateTime : DateTime.UtcNow;
            _logger.LogMetric(EventMetrics.EventTimestampLastProcessedPerPartition(partitionId, triggerReason), double.Parse(eventTimestampLastProcessed.ToString("yyyyMMddHHmmss")));
        }
    }
}
