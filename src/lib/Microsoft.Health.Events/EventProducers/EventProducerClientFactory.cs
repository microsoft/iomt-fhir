// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using EnsureThat;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;

namespace Microsoft.Health.Events.EventProducers
{
    public class EventProducerClientFactory : IEventProducerClientFactory
    {
        public EventHubProducerClient GetEventHubProducerClient(EventProducerClientOptions options, IAzureCredentialProvider provider = null)
        {
            EnsureArg.IsNotNull(options);

            if (options.ServiceManagedIdentityAuth)
            {
                var tokenCredential = new DefaultAzureCredential();
                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                return new EventHubProducerClient(eventHubFQDN, options.EventHubName, tokenCredential);
            }
            else if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                return new EventHubProducerClient(options.ConnectionString);
            }
            else if (provider != null)
            {
                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                return new EventHubProducerClient(eventHubFQDN, options.EventHubName, provider.GetCredential());
            }
            else
            {
                throw new Exception($"Unable to create Event Hub producer client for {options.EventHubName}");
            }
        }
    }
}
