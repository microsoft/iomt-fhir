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

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class EventProcessorClientFactory : IEventProcessorClientFactory
    {
        public EventProcessorClient CreateProcessorClient(BlobContainerClient blobContainerClient, EventProcessorClientFactoryOptions options, EventProcessorClientOptions eventProcessorClientOptions)
        {
            EnsureArg.IsNotNull(blobContainerClient);
            EnsureArg.IsNotNull(options);
            EnsureArg.IsNotNull(eventProcessorClientOptions);

            if (options.ServiceManagedIdentityAuth)
            {
                var tokenCredential = new DefaultAzureCredential();
                return new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, options.EventHubNamespaceFQDN, options.EventHubName, tokenCredential, eventProcessorClientOptions);
            }
            else if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                return new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, options.ConnectionString, options.EventHubName, eventProcessorClientOptions);
            }
            else
            {
                throw new Exception($"Unable to create Event Hub processor client for {options.EventHubName}");
            }
        }

        public EventProcessorClient CreateProcessorClient(IAzureCredentialProvider provider, BlobContainerClient blobContainerClient, EventProcessorClientFactoryOptions options, EventProcessorClientOptions eventProcessorClientOptions)
        {
            EnsureArg.IsNotNull(provider);
            EnsureArg.IsNotNull(blobContainerClient);
            EnsureArg.IsNotNull(options);
            EnsureArg.IsNotNull(eventProcessorClientOptions);

            var tokenCredential = provider.GetCredential();
            return new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, options.EventHubNamespaceFQDN, options.EventHubName, tokenCredential, eventProcessorClientOptions);
        }
    }
}
