// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.EventConsumers.Service
{
    public class EventConsumerService : IEventConsumerService
    {
        private readonly IEnumerable<IEventConsumer> eventConsumers;

        public EventConsumerService(IEnumerable<IEventConsumer> eventConsumers)
        {
            this.eventConsumers = eventConsumers;
        }

        public Task ConsumeEvent(Event eventArg)
        {
            throw new System.NotImplementedException();
        }

        public Task ConsumeEvents(IEnumerable<Event> events)
        {
            foreach (IEventConsumer eventConsumer in eventConsumers)
            {
                eventConsumer.ConsumeAsync(events);
            }

            return Task.CompletedTask;
        }
    }
}
