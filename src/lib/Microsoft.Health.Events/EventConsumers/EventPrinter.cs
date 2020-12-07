// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.EventConsumers
{
    public class EventPrinter : IEventConsumer
    {
        public Task ConsumeAsync(IEnumerable<IEventMessage> events)
        {
            EnsureArg.IsNotNull(events);
            foreach (EventMessage evt in events)
            {
                string message = Encoding.UTF8.GetString(evt.Body.ToArray());
                var enqueuedTime = evt.EnqueuedTime.UtcDateTime;
                Console.WriteLine($"Enqueued Time: {enqueuedTime} Event Message: \"{message}\"");
            }

            return Task.CompletedTask;
        }
    }
}
