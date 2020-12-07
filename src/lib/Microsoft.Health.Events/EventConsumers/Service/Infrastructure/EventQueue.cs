// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logger.Telemetry;

namespace Microsoft.Health.Events.EventConsumers.Service.Infrastructure
{
    public class EventQueue
    {
        private string _queueId;
        private ConcurrentQueue<IEventMessage> _queue;
        private EventQueueWindow _queueWindow;
        private ITelemetryLogger _logger;

        public EventQueue(string queueId, DateTime initDateTime, TimeSpan flushTimespan, ITelemetryLogger logger)
        {
            _queueId = queueId;
            _queue = new ConcurrentQueue<IEventMessage>();
            _queueWindow = new EventQueueWindow(initDateTime, flushTimespan);
            _logger = logger;
        }

        public void IncrementQueueWindow(DateTime dateTime)
        {
            _queueWindow.IncrementWindow(dateTime);
        }

        public DateTime GetQueueWindow()
        {
            return _queueWindow.GetWindowEnd();
        }

        public int GetQueueCount()
        {
            return _queue.Count;
        }

        public void Enqueue(IEventMessage eventArg)
        {
            _queue.Enqueue(eventArg);
        }

        // flush a fixed number of events
        public Task<List<IEventMessage>> Flush(int numEvents)
        {
            Console.WriteLine($"Flushing {numEvents} events");

            var count = 0;
            var events = new List<IEventMessage>();

            while (count < numEvents)
            {
                if (_queue.TryDequeue(out var dequeuedEvent))
                {
                    events.Add(dequeuedEvent);
                    count++;
                }
            }

            Console.WriteLine($"Current window {GetQueueWindow()}");
            return Task.FromResult(events);
        }

        // flush up to a date time
        public Task<List<IEventMessage>> Flush(DateTime dateTime)
        {
            var events = new List<IEventMessage>();
            while (_queue.TryPeek(out var eventData))
            {
                var enqueuedUtc = eventData.EnqueuedTime.UtcDateTime;
                if (enqueuedUtc <= dateTime)
                {
                    _queue.TryDequeue(out var dequeuedEvent);
                    events.Add(dequeuedEvent);
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine($"Flushed {events.Count} events up to {dateTime}");
            return Task.FromResult(events);
        }
    }
}