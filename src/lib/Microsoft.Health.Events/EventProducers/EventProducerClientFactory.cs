// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using EnsureThat;

namespace Microsoft.Health.Events.EventProducers
{
    public class EventProducerClientFactory : IEventProducerClientFactory
    {
        public EventHubProducerClient GetEventHubProducerClient(EventProducerClientOptions options)
        {
            EnsureArg.IsNotNull(options);
            if (options.ServiceManagedIdentityAuth)
            {
                var tokenCredential = new DefaultAzureCredential();
                return new EventHubProducerClient(options.EventHubNamespaceFQDN, options.EventHubName, tokenCredential);
            }
            else if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                return new EventHubProducerClient(options.ConnectionString, options.EventHubName);
            }
            else
            {
                throw new Exception($"Unable to create Event Hub processor client for {options.EventHubName}");
            }
        }
    }
}
