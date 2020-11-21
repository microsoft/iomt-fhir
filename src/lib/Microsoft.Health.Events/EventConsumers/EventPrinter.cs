// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.EventConsumers
{
    public class EventPrinter : IEventConsumer
    {
        public async Task<IActionResult> ConsumeAsync(IEnumerable<Event> events)
        {
            EnsureArg.IsNotNull(events);
            foreach (Event evt in events)
            {
                string message = Encoding.UTF8.GetString(evt.Body.ToArray());
                var enqueuedTime = evt.EnqueuedTime.UtcDateTime;
                Console.WriteLine($"Enqueued Time: {enqueuedTime} Event Message: \"{message}\"");
            }

            return await Task.FromResult(new AcceptedResult());
        }
    }
}
