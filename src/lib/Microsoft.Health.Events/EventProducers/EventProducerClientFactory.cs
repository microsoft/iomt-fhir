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
        public EventHubProducerClient GetEventHubProducerClient(EventHubClientOptions options, IAzureCredentialProvider provider = null)
        {
            EnsureArg.IsNotNull(options);

            if (options.AuthenticationType == AuthenticationType.ManagedIdentity)
            {
                EnsureArg.IsNotNull(options.EventHubName);
                EnsureArg.IsNotNull(options.EventHubNamespaceFQDN);

                var tokenCredential = new DefaultAzureCredential();
                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                return new EventHubProducerClient(eventHubFQDN, options.EventHubName, tokenCredential);
            }
            else if (options.AuthenticationType == AuthenticationType.ConnectionString)
            {
                EnsureArg.IsNotNull(options.ConnectionString);

                return new EventHubProducerClient(options.ConnectionString);
            }
            else if (options.AuthenticationType == AuthenticationType.Custom)
            {
                EnsureArg.IsNotNull(options.EventHubName);
                EnsureArg.IsNotNull(options.EventHubNamespaceFQDN);
                EnsureArg.IsNotNull(provider);

                var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                return new EventHubProducerClient(eventHubFQDN, options.EventHubName, provider.GetCredential());
            }
            else
            {
                var ex = $"Unable to create Event Hub producer client for {options.EventHubName}";
                var message = "No authentication type was specified for EventHubClientOptions.";
                throw new Exception($"{ex} {message}");
            }
        }
    }
}
