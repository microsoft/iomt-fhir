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
        private ConcurrentDictionary<string, EventQueue> _eventQueues;
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

            _eventQueues = new ConcurrentDictionary<string, EventQueue>();
            _eventConsumerService = eventConsumerService;
            _maxEvents = options.MaxEvents;
            _flushTimespan = TimeSpan.FromSeconds(options.FlushTimespan);
            _checkpointClient = checkpointClient;
            _logger = logger;
        }

        public EventQueue GetQueue(string queueId)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueId);

            if (!_eventQueues.ContainsKey(queueId))
            {
                throw new Exception($"Queue with identifier {queueId} does not exist");
            }

            return _eventQueues[queueId];
        }

        private bool EventQueueExists(string queueId)
        {
            return _eventQueues.ContainsKey(queueId);
        }

        private EventQueue CreateQueueIfMissing(string queueId, DateTime initTime, TimeSpan flushTimespan)
        {
            return _eventQueues.GetOrAdd(queueId, new EventQueue(queueId, initTime, flushTimespan, _logger));
        }

        public Task ConsumeEvent(IEventMessage eventArg)
        {
            EnsureArg.IsNotNull(eventArg);

            var queueId = eventArg.PartitionId;
            var eventEnqueuedTime = eventArg.EnqueuedTime.UtcDateTime;

            if (eventArg is MaximumWaitEvent)
            {
                if (EventQueueExists(queueId))
                {
                    var windowThresholdTime = GetQueue(queueId).GetQueueWindow();
                    ThresholdWaitReached(queueId, windowThresholdTime);
                }
            }
            else
            {
                var queue = CreateQueueIfMissing(queueId, eventEnqueuedTime, _flushTimespan);

                queue.Enqueue(eventArg);

                var windowThresholdTime = queue.GetQueueWindow();
                if (eventEnqueuedTime > windowThresholdTime)
                {
                    ThresholdTimeReached(queueId, eventArg, windowThresholdTime);
                    return Task.CompletedTask;
                }

                if (queue.GetQueueCount() >= _maxEvents)
                {
                    ThresholdCountReached(queueId);
                }
            }

            return Task.CompletedTask;
        }

        // todo: fix -"Collection was modified; enumeration operation may not execute."
        private async void ThresholdCountReached(string queueId)
        {
            _logger.LogTrace($"Partition {queueId} threshold count {_maxEvents} was reached.");
            var events = await GetQueue(queueId).Flush(_maxEvents);
            await _eventConsumerService.ConsumeEvents(events);
            UpdateCheckpoint(events);
        }

        private async void ThresholdTimeReached(string queueId, IEventMessage eventArg, DateTime windowEnd)
        {
            _logger.LogTrace($"Partition {queueId} threshold time {_eventQueues[queueId].GetQueueWindow()} was reached.");
            var queue = GetQueue(queueId);
            var events = await queue.Flush(windowEnd);
            await _eventConsumerService.ConsumeEvents(events);
            queue.IncrementQueueWindow(eventArg.EnqueuedTime.UtcDateTime);
            UpdateCheckpoint(events);
        }

        private async void ThresholdWaitReached(string queueId, DateTime windowEnd)
        {
            if (windowEnd < DateTime.UtcNow.AddSeconds(_timeBuffer))
            {
                _logger.LogTrace($"Partition {queueId} threshold wait reached. Flushing {_eventQueues[queueId].GetQueueCount()} events up to: {windowEnd}");
                var events = await GetQueue(queueId).Flush(windowEnd);
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
