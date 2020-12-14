// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service.Infrastructure;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logger.Telemetry;

namespace Microsoft.Health.Events.EventConsumers.Service
{
    public class EventBatchingService : IEventConsumerService
    {
        private ConcurrentDictionary<string, EventPartition> _eventPartitions;
        private int _maxEvents;
        private TimeSpan _flushTimespan;
        private IEventConsumerService _eventConsumerService;
        private ICheckpointClient _checkpointClient;
        private ITelemetryLogger _logger;
        private const int _timeBuffer = -5;

        public EventBatchingService(IEventConsumerService eventConsumerService, EventBatchingOptions options, ICheckpointClient checkpointClient, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(options);
            EnsureArg.IsInt(options.MaxEvents);
            EnsureArg.IsInt(options.FlushTimespan);

            _eventPartitions = new ConcurrentDictionary<string, EventPartition>();
            _eventConsumerService = eventConsumerService;
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

        public Task ConsumeEvent(IEventMessage eventArg)
        {
            EnsureArg.IsNotNull(eventArg);

            var partitionId = eventArg.PartitionId;
            var eventEnqueuedTime = eventArg.EnqueuedTime.UtcDateTime;

            if (eventArg is MaximumWaitEvent)
            {
                if (EventPartitionExists(partitionId))
                {
                    var windowThresholdTime = GetPartition(partitionId).GetPartitionWindow();
                    ThresholdWaitReached(partitionId, windowThresholdTime);
                }
            }
            else
            {
                var partition = CreatePartitionIfMissing(partitionId, eventEnqueuedTime, _flushTimespan);

                partition.Enqueue(eventArg);

                var windowThresholdTime = partition.GetPartitionWindow();
                if (eventEnqueuedTime > windowThresholdTime)
                {
                    ThresholdTimeReached(partitionId, eventArg, windowThresholdTime);
                    return Task.CompletedTask;
                }

                if (partition.GetPartitionBatchCount() >= _maxEvents)
                {
                    ThresholdCountReached(partitionId);
                }
            }

            return Task.CompletedTask;
        }

        // todo: fix -"Collection was modified; enumeration operation may not execute."
        private async void ThresholdCountReached(string partitionId)
        {
            _logger.LogTrace($"Partition {partitionId} threshold count {_maxEvents} was reached.");
            var events = await GetPartition(partitionId).Flush(_maxEvents);
            await _eventConsumerService.ConsumeEvents(events);
            UpdateCheckpoint(events);
        }

        private async void ThresholdTimeReached(string partitionId, IEventMessage eventArg, DateTime windowEnd)
        {
            _logger.LogTrace($"Partition {partitionId} threshold time {_eventPartitions[partitionId].GetPartitionWindow()} was reached.");
            var queue = GetPartition(partitionId);
            var events = await queue.Flush(windowEnd);
            queue.IncrementPartitionWindow(eventArg.EnqueuedTime.UtcDateTime);
            await _eventConsumerService.ConsumeEvents(events);
            UpdateCheckpoint(events);
        }

        private async void ThresholdWaitReached(string partitionId, DateTime windowEnd)
        {
            if (windowEnd < DateTime.UtcNow.AddSeconds(_timeBuffer))
            {
                _logger.LogTrace($"Partition {partitionId} threshold wait reached. Flushing {_eventPartitions[partitionId].GetPartitionBatchCount()} events up to: {windowEnd}");
                var events = await GetPartition(partitionId).Flush(windowEnd);
                await _eventConsumerService.ConsumeEvents(events);
                UpdateCheckpoint(events);
            }
        }

        private async void UpdateCheckpoint(List<IEventMessage> events)
        {
            if (events.Count > 0)
            {
                var eventCheckpoint = events[events.Count - 1];
                await _checkpointClient.SetCheckpointAsync(eventCheckpoint);
            }
        }

        public Task ConsumeEvents(IEnumerable<IEventMessage> events)
        {
            throw new NotImplementedException();
        }
    }
}
