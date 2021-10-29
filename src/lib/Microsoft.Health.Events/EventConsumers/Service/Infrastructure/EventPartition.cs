// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventConsumers.Service.Infrastructure
{
    public class EventPartition
    {
        private string _partitionId;
        private ConcurrentQueue<IEventMessage> _partition;
        private DateTime _partitionWindow;
        private TimeSpan _flushTimespan;
        private ITelemetryLogger _logger;

        public EventPartition(string partitionId, DateTime initDateTime, TimeSpan flushTimespan, ITelemetryLogger logger)
        {
            _partitionId = partitionId;
            _partition = new ConcurrentQueue<IEventMessage>();
            _partitionWindow = initDateTime.Add(flushTimespan);
            _flushTimespan = flushTimespan;
            _logger = logger;
        }

        public void Enqueue(IEventMessage eventArg)
        {
            _partition.Enqueue(eventArg);
        }

        public void IncrementPartitionWindow(DateTime dateTime)
        {
            // todo: consider computing instead of while loop.
            while (dateTime >= _partitionWindow)
            {
                _partitionWindow = _partitionWindow.Add(_flushTimespan);
            }
        }

        public DateTime GetPartitionWindow()
        {
            return _partitionWindow;
        }

        public int GetPartitionBatchCount()
        {
            return _partition.Count;
        }

        // flush a fixed number of events
        public Task<List<IEventMessage>> Flush(int numEvents)
        {
            var count = 0;
            var events = new List<IEventMessage>();

            while (count < numEvents)
            {
                if (_partition.TryDequeue(out var dequeuedEvent))
                {
                    events.Add(dequeuedEvent);
                    count++;
                }
            }

            _logger.LogTrace($"Flushed {events.Count} events on partition {_partitionId}");
            _logger.LogMetric(EventMetrics.EventsFlushed(_partitionId), events.Count);
            return Task.FromResult(events);
        }

        // flush up to a date time
        public Task<List<IEventMessage>> Flush(DateTime dateTime)
        {
            var events = new List<IEventMessage>();
            while (_partition.TryPeek(out var eventData))
            {
                var enqueuedUtc = eventData.EnqueuedTime.UtcDateTime;
                if (enqueuedUtc <= dateTime)
                {
                    _partition.TryDequeue(out var dequeuedEvent);
                    events.Add(dequeuedEvent);
                }
                else
                {
                    break;
                }
            }

            _logger.LogTrace($"Flushed {events.Count} events up to {dateTime} on partition {_partitionId}");
            _logger.LogMetric(EventMetrics.EventsFlushed(_partitionId), events.Count);
            return Task.FromResult(events);
        }
    }
}