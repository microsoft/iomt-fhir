// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.EventConsumers.Service.Infrastructure
{
    public class EventQueue
    {
        private string _queueId;
        private ConcurrentQueue<Event> _queue;
        private EventQueueWindow _queueWindow;

        public EventQueue(string queueId, DateTime initDateTime, TimeSpan flushTimespan)
        {
            _queueId = queueId;
            _queue = new ConcurrentQueue<Event>();
            _queueWindow = new EventQueueWindow(initDateTime, flushTimespan);
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

        public void Enqueue(Event eventArg)
        {
            _queue.Enqueue(eventArg);
        }

        // flush a fixed number of events
        public Task<List<Event>> Flush(int numEvents)
        {
            Console.WriteLine($"Flushing {numEvents} events");

            var count = 0;
            var events = new List<Event>();

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
        public Task<List<Event>> Flush(DateTime dateTime)
        {
            Console.WriteLine($"Attempt to flush queue up to {dateTime}");

            var events = new List<Event>();
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