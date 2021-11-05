// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class EventHubProducerService : IEventHubMessageService
    {
        private readonly EventHubProducerClient _client;

        public EventHubProducerService(EventHubProducerClient client)
        {
            _client = EnsureArg.IsNotNull(client, nameof(client));
        }

        public async Task CloseAsync()
        {
            await _client.CloseAsync().ConfigureAwait(false);
        }

        public async ValueTask<EventDataBatch> CreateEventDataBatchAsync(string partitionKey)
        {
            EnsureArg.IsNotNullOrWhiteSpace(partitionKey, nameof(partitionKey));
            return await _client.CreateBatchAsync(new CreateBatchOptions()
            {
                PartitionKey = partitionKey,
            });
        }

        public async Task SendAsync(EventDataBatch eventData, CancellationToken token)
        {
            await _client.SendAsync(eventData, token);
        }

        public async Task SendAsync(IEnumerable<EventData> eventData, CancellationToken token)
        {
            await _client.SendAsync(eventData, token);
        }

        public async Task SendAsync(IEnumerable<EventData> eventData,  string partitionKey, CancellationToken token)
        {
            var options = new SendEventOptions();
            options.PartitionKey = partitionKey;
            await _client.SendAsync(eventData, options, token);
        }
    }
}
