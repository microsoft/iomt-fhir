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
using Azure.Identity;
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

        public EventConsumerService(
            IEnumerable<IEventConsumer> eventConsumers,
            ITelemetryLogger logger,
            bool shouldRetry = true,
            Action<Exception> exceptionTelemetryProcessor = null)
        {
            _eventConsumers = EnsureArg.IsNotNull(eventConsumers, nameof(eventConsumers));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));

            RetryPolicy = CreateRetryPolicy(logger, shouldRetry, exceptionTelemetryProcessor);
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
                        await RetryPolicy.ExecuteAsync(async () => await eventConsumer.ConsumeAsync(events).ConfigureAwait(false));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e);
                    }
                }
            }
        }

        private static AsyncPolicy CreateRetryPolicy(ITelemetryLogger logger, bool shouldRetry, Action<Exception> exceptionTelemetryProcessor)
        {
            bool ExceptionRetryableFilter(Exception ee)
            {
                if (exceptionTelemetryProcessor != null)
                {
                    exceptionTelemetryProcessor.Invoke(ee);
                }

                if (!shouldRetry)
                {
                    return false;
                }

                switch (ee)
                {
                    case AggregateException ae when ae.InnerExceptions.Any(ExceptionRetryableFilter):
                    case OperationCanceledException _:
                    case HttpRequestException _:
                    case EventHubsException _:
                    case AuthenticationFailedException _:
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
