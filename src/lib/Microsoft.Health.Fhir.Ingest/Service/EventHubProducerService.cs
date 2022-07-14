// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using EnsureThat;
using Microsoft.Health.Logging.Telemetry;
using Polly;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class EventHubProducerService : IEventHubMessageService
    {
        private readonly EventHubProducerClient _client;
        private ITelemetryLogger _logger;
        private readonly AsyncPolicy _retryPolicy;

        public EventHubProducerService(EventHubProducerClient client, ITelemetryLogger logger)
        {
            _client = EnsureArg.IsNotNull(client, nameof(client));
            _logger = logger;
            _retryPolicy = CreateRetryPolicy();
        }

        private AsyncPolicy CreateRetryPolicy()
        {
            return Policy
                .Handle<Exception>(exceptionPredicate: ex => LogRetry(ex))
                .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)));
        }

        private bool LogRetry(Exception ex)
        {
            _logger.LogTrace($"Sending request to Event Hub using {nameof(EventHubProducerService)} failed. {ex}. Retrying.");
            return true;
        }

        public async Task CloseAsync()
        {
            await _client.CloseAsync().ConfigureAwait(false);
        }

        public async ValueTask<EventDataBatch> CreateEventDataBatchAsync(string partitionKey)
        {
            EnsureArg.IsNotNullOrWhiteSpace(partitionKey, nameof(partitionKey));

            try
            {
                var policyResult = await _retryPolicy
                    .ExecuteAndCaptureAsync(async () => await _client.CreateBatchAsync(new CreateBatchOptions()
                    {
                        PartitionKey = partitionKey,
                    }));

                if (policyResult.FinalException != null)
                {
                    throw policyResult.FinalException;
                }

                return policyResult.Result;
            }
            catch (Exception ex)
            {
                throw new EventHubProducerClientException(ex.Message, ex, nameof(EventHubProducerClientException));
            }
        }

        public async Task SendAsync(EventDataBatch eventData, CancellationToken token)
        {
            try
            {
                var policyResult = await _retryPolicy
                    .ExecuteAndCaptureAsync(async () => await _client.SendAsync(eventData, token));

                if (policyResult.FinalException != null)
                {
                    throw policyResult.FinalException;
                }
            }
            catch (Exception ex)
            {
                throw new EventHubProducerClientException(ex.Message, ex, nameof(EventHubProducerClientException));
            }
        }

        public async Task SendAsync(IEnumerable<EventData> eventData, CancellationToken token)
        {
            try
            {
                var policyResult = await _retryPolicy
                    .ExecuteAndCaptureAsync(async () => await _client.SendAsync(eventData, token));

                if (policyResult.FinalException != null)
                {
                    throw policyResult.FinalException;
                }
            }
            catch (Exception ex)
            {
                throw new EventHubProducerClientException(ex.Message, ex, nameof(EventHubProducerClientException));
            }
        }

        public async Task SendAsync(IEnumerable<EventData> eventData,  string partitionKey, CancellationToken token)
        {
            try
            {
                var policyResult = await _retryPolicy
                    .ExecuteAndCaptureAsync(async () =>
                    {
                        var options = new SendEventOptions();
                        options.PartitionKey = partitionKey;
                        await _client.SendAsync(eventData, options, token);
                    });

                if (policyResult.FinalException != null)
                {
                    throw policyResult.FinalException;
                }
            }
            catch (Exception ex)
            {
                throw new EventHubProducerClientException(ex.Message, ex, nameof(EventHubProducerClientException));
            }
        }
    }
}
