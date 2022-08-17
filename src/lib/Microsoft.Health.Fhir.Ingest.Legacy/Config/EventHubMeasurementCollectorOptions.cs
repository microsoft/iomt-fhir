// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using EnsureThat;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Health.Fhir.Ingest.Config
{
    public class EventHubMeasurementCollectorOptions
    {
        private readonly ConcurrentDictionary<string, EventHubClient> _clients = new ConcurrentDictionary<string, EventHubClient>(StringComparer.OrdinalIgnoreCase);

        public EventHubClient GetEventHubClient(string eventHubName, string connection)
        {
            EnsureArg.IsNotNullOrWhiteSpace(eventHubName, nameof(eventHubName));

            if (_clients.TryGetValue(eventHubName, out var client))
            {
                return client;
            }
            else if (!string.IsNullOrWhiteSpace(connection))
            {
                return _clients.GetOrAdd(eventHubName, key => CreateClient(key, connection));
            }

            throw new InvalidOperationException($"Event hub connection named {eventHubName} is not defined and no connection string was provided.");
        }

        public void AddSender(string eventHubName, string connection)
        {
            _clients[eventHubName] = CreateClient(eventHubName, connection);
        }

        private static EventHubClient CreateClient(string eventHubName, string connection)
        {
            EnsureArg.IsNotNullOrWhiteSpace(eventHubName, nameof(eventHubName));
            EnsureArg.IsNotNullOrWhiteSpace(connection, nameof(connection));

            var sb = new EventHubsConnectionStringBuilder(connection);
            if (string.IsNullOrWhiteSpace(sb.EntityPath))
            {
                sb.EntityPath = eventHubName;
            }

            return EventHubClient.CreateFromConnectionString(sb.ToString());
        }
    }
}