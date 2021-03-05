// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using EnsureThat;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class EventProcessorClientFactory : IEventProcessorClientFactory
    {
        public EventProcessorClient CreateProcessorClient(BlobContainerClient blobContainerClient, EventProcessorClientFactoryOptions options, EventProcessorClientOptions eventProcessorClientOptions, IAzureCredentialProvider provider = null)
        {
            EnsureArg.IsNotNull(blobContainerClient);
            EnsureArg.IsNotNull(options);
            EnsureArg.IsNotNull(eventProcessorClientOptions);

            if (options.ServiceManagedIdentityAuth)
            {
                var tokenCredential = new DefaultAzureCredential();
                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                return new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, eventHubFQDN, options.EventHubName, tokenCredential, eventProcessorClientOptions);
            }
            else if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                return new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, options.ConnectionString, eventProcessorClientOptions);
            }
            else if (provider != null)
            {
                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                return new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, eventHubFQDN, options.EventHubName, provider.GetCredential(), eventProcessorClientOptions);
            }
            else
            {
                var ex = $"Unable to create Event Hub processor client for {options.EventHubName}.";
                var message = "No valid authentication configuration options were found. ServiceManagedIdentityAuth is not enabled, No ConnectionString specified, No Token Provider provided.";
                throw new Exception($"{ex} {message}");
            }
        }
    }
}
