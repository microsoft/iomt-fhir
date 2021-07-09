// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventHubs;
using EnsureThat;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logging.Telemetry;
using Polly;

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

            RetryPolicy = CreateRetryPolicy(logger);
        }

        public EventConsumerService(IEnumerable<IEventConsumer> eventConsumers, ITelemetryLogger logger, AsyncPolicy retryPolicy)
        {
            _eventConsumers = EnsureArg.IsNotNull(eventConsumers, nameof(eventConsumers));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            RetryPolicy = EnsureArg.IsNotNull(retryPolicy, nameof(retryPolicy));
        }

        public AsyncPolicy RetryPolicy { get;  }

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
                        await RetryPolicy.ExecuteAsync(async () => await TryOperationAsync(eventConsumer, events).ConfigureAwait(false));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e);
                    }
                }
            }
        }

        private static async Task TryOperationAsync(IEventConsumer eventConsumer, IEnumerable<IEventMessage> events)
        {
            await eventConsumer.ConsumeAsync(events);
        }

        private static AsyncPolicy CreateRetryPolicy(ITelemetryLogger logger)
        {
            bool ExceptionRetryableFilter(Exception ee)
            {
                switch (ee)
                {
                    case AggregateException ae when ae.InnerExceptions.All(ExceptionRetryableFilter):
                    case OperationCanceledException _:
                    case HttpRequestException _:
                    case EventHubsException _:
                    case RequestFailedException _:
                        break;
                    default:
                        return false;
                }

                logger.LogError(new Exception("Encountered retryable exception", ee));
                return true;
            }

            return Policy
                .Handle<Exception>(ExceptionRetryableFilter)
                .WaitAndRetryForeverAsync(retryCount => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, retryCount))));
        }
    }
}
