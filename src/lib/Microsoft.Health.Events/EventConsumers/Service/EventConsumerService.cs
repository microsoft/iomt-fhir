// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventConsumers.Service
{
    public class EventConsumerService : IEventConsumerService
    {
        private readonly IEnumerable<IEventConsumer> _eventConsumers;
        private ITelemetryLogger _logger;

        public EventConsumerService(IEnumerable<IEventConsumer> eventConsumers, ITelemetryLogger logger)
        {
            _eventConsumers = EnsureArg.IsNotNull(eventConsumers, nameof(eventConsumers));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
        }

        public Task ConsumeEvent(IEventMessage eventArg)
        {
            throw new NotImplementedException();
        }

        public async Task ConsumeEvents(IEnumerable<IEventMessage> events)
        {
            if (events.Any())
            {
                foreach (IEventConsumer eventConsumer in _eventConsumers)
                {
                    try
                    {
                        await eventConsumer.ConsumeAsync(events).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e);
                    }
                }
            }
        }
    }
}
