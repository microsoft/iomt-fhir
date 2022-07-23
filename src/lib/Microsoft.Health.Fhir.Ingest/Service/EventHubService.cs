// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class EventHubService : IEventHubService
    {
        private readonly EventHubProducerClient _client;

        public EventHubService(EventHubProducerClient client)
        {
            _client = EnsureArg.IsNotNull(client, nameof(client));
        }

        public async Task CloseAsync()
        {
            await _client.CloseAsync().ConfigureAwait(false);
        }

        public async Task SendAsync(EventData eventData)
        {
            await _client.SendAsync(ToIEnumerable(eventData)).ConfigureAwait(false);
        }

        public async Task SendAsync(EventData eventData, string partitionKey)
        {
            var options = new SendEventOptions
            {
                PartitionKey = partitionKey,
            };

            await _client.SendAsync(ToIEnumerable(eventData), options).ConfigureAwait(false);
        }

        public async Task SendAsync(IEnumerable<EventData> eventData, string partitionKey)
        {
            var options = new SendEventOptions
            {
                PartitionKey = partitionKey,
            };

            await _client.SendAsync(eventData, options).ConfigureAwait(false);
        }

        private static IEnumerable<EventData> ToIEnumerable(EventData eventData)
        {
            yield return eventData;
        }
    }
}
