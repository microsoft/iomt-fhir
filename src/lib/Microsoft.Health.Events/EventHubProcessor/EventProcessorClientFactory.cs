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
    public class EventProcessorClientFactory
    {
        private BlobContainerClient _blobContainerClient;

        public EventProcessorClientFactory(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = EnsureArg.IsNotNull(blobContainerClient, nameof(blobContainerClient));
        }

        public EventProcessorClient CreateProcessorClient(EventProcessorClientFactoryOptions options, EventProcessorClientOptions eventProcessorClientOptions)
        {
            EnsureArg.IsNotNull(options);

            if (options.ServiceManagedIdentityAuth)
            {
                var tokenCredential = new DefaultAzureCredential();
                return new EventProcessorClient(_blobContainerClient, options.EventHubConsumerGroup, options.EventHubNamespaceFQDN, options.EventHubName, tokenCredential, eventProcessorClientOptions);
            }
            else if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                return new EventProcessorClient(_blobContainerClient, options.EventHubConsumerGroup, options.ConnectionString, options.EventHubName, eventProcessorClientOptions);
            }
            else
            {
                throw new Exception($"Unable to create Event Hub processor client for {options.EventHubName}");
            }
        }

        public EventProcessorClient CreateProcessorClient(IAzureCredentialProvider provider, EventProcessorClientFactoryOptions options, EventProcessorClientOptions eventProcessorClientOptions)
        {
            EnsureArg.IsNotNull(options);
            EnsureArg.IsNotNull(provider);

            var tokenCredential = provider.GetCredential();
            return new EventProcessorClient(_blobContainerClient, options.EventHubConsumerGroup, options.EventHubNamespaceFQDN, options.EventHubName, tokenCredential, eventProcessorClientOptions);
        }
    }
}
