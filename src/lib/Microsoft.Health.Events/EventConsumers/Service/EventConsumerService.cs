// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.EventConsumers.Service
{
    public class EventConsumerService : IEventConsumerService
    {
        private readonly IEnumerable<IEventConsumer> eventConsumers;
        private const int _maximumBackoffMs = 32000;

        public EventConsumerService(IEnumerable<IEventConsumer> eventConsumers)
        {
            this.eventConsumers = eventConsumers;
        }

        public Task ConsumeEvent(IEventMessage eventArg)
        {
            throw new NotImplementedException();
        }

        public async Task ConsumeEvents(IEnumerable<IEventMessage> events)
        {
            foreach (IEventConsumer eventConsumer in eventConsumers)
            {
                await OperationWithRetryAsync(eventConsumer, events);
            }
        }

        private static async Task OperationWithRetryAsync(IEventConsumer eventConsumer, IEnumerable<IEventMessage> events)
        {
            int currentRetry = 0;
            double backoffMs = 0;
            Random random = new Random();
            bool operationComplete = false;

            while (!operationComplete)
            {
                try
                {
                    if (currentRetry > 0 && backoffMs < _maximumBackoffMs)
                    {
                        int randomMs = random.Next(0, 1000);
                        backoffMs = Math.Pow(2000, currentRetry) + randomMs;
                        await Task.Delay((int)backoffMs);
                    }

                    await TryOperationAsync(eventConsumer, events).ConfigureAwait(false);
                    break;
                }
#pragma warning disable CA1031
                catch (Exception e)
#pragma warning restore CA1031
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static async Task TryOperationAsync(IEventConsumer eventConsumer, IEnumerable<IEventMessage> events)
        {
            await eventConsumer.ConsumeAsync(events);
        }
    }
}
