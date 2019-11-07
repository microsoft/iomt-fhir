// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class EventHubService : IEventHubService
    {
        private readonly EventHubClient _client;

        public EventHubService(EventHubClient client)
        {
            _client = EnsureArg.IsNotNull(client, nameof(client));
        }

        public async Task CloseAsync()
        {
            await _client.CloseAsync().ConfigureAwait(false);
        }

        public async Task SendAsync(EventData eventData)
        {
            await _client.SendAsync(eventData).ConfigureAwait(false);
        }

        public async Task SendAsync(EventData eventData, string partitionKey)
        {
            await _client.SendAsync(eventData, partitionKey).ConfigureAwait(false);
        }

        public async Task SendAsync(IEnumerable<EventData> eventData, string partitionKey)
        {
            await _client.SendAsync(eventData, partitionKey).ConfigureAwait(false);
        }
    }
}
