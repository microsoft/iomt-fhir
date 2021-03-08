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
        public EventProcessorClient CreateProcessorClient(BlobContainerClient blobContainerClient, EventHubClientOptions options, EventProcessorClientOptions eventProcessorClientOptions, IAzureCredentialProvider provider = null)
        {
            EnsureArg.IsNotNull(blobContainerClient);
            EnsureArg.IsNotNull(eventProcessorClientOptions);
            EnsureArg.IsNotNull(options);
            EnsureArg.IsNotNull(options.EventHubConsumerGroup, nameof(options.EventHubConsumerGroup));

            if (options.AuthenticationType == AuthenticationType.ManagedIdentity)
            {
                EnsureArg.IsNotNull(options.EventHubNamespaceFQDN);
                EnsureArg.IsNotNull(options.EventHubName);

                var tokenCredential = new DefaultAzureCredential();
                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                return new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, eventHubFQDN, options.EventHubName, tokenCredential, eventProcessorClientOptions);
            }
            else if (options.AuthenticationType == AuthenticationType.ConnectionString)
            {
                EnsureArg.IsNotNull(options.ConnectionString);
                return new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, options.ConnectionString, eventProcessorClientOptions);
            }
            else if (options.AuthenticationType == AuthenticationType.Custom)
            {
                EnsureArg.IsNotNull(options.EventHubNamespaceFQDN);
                EnsureArg.IsNotNull(options.EventHubName);
                EnsureArg.IsNotNull(provider);

                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                return new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, eventHubFQDN, options.EventHubName, provider.GetCredential(), eventProcessorClientOptions);
            }
            else
            {
                var ex = $"Unable to create Event Hub processor client for {options.EventHubName}.";
                var message = "No authentication type was specified for EventHubClientOptions";
                throw new Exception($"{ex} {message}");
            }
        }
    }
}
