// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using Azure.Messaging.EventHubs.Producer;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Config
{
    public class EventHubMeasurementCollectorOptions
    {
        private readonly ConcurrentDictionary<string, EventHubProducerClient> _clients = new (StringComparer.OrdinalIgnoreCase);

        public EventHubProducerClient GetEventHubClient(string eventHubName, string connection)
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

        private static EventHubProducerClient CreateClient(string eventHubName, string connection)
        {
            EnsureArg.IsNotNullOrWhiteSpace(eventHubName, nameof(eventHubName));
            EnsureArg.IsNotNullOrWhiteSpace(connection, nameof(connection));

            // Check if entity path is defined in the connection string
            var entityToken = connection.Split(';').FirstOrDefault(t => t.StartsWith("EntityPath="));

            if (entityToken == null)
            {
                return new EventHubProducerClient(connection, eventHubName);
            }
            else
            {
                return new EventHubProducerClient(connection);
            }
        }
    }
}