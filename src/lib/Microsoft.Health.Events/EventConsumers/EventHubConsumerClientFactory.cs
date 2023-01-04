// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Identity;
using Azure.Messaging.EventHubs.Consumer;
using EnsureThat;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;

namespace Microsoft.Health.Events.EventConsumers
{
    public class EventHubConsumerClientFactory : IEventHubConsumerClientFactory
    {
        public EventHubConsumerClient CreateConsumerClient(EventHubClientOptions options, EventHubConsumerClientOptions eventHubConsumerClientOptions, IAzureCredentialProvider provider)
        {
            EnsureArg.IsNotNull(options?.EventHubConsumerGroup, nameof(options.EventHubConsumerGroup));
            EnsureArg.IsNotNull(eventHubConsumerClientOptions, nameof(eventHubConsumerClientOptions));

            switch (options.AuthenticationType)
            {
                case AuthenticationType.ManagedIdentity:
                    EnsureArg.IsNotNull(options.EventHubNamespaceFQDN);
                    EnsureArg.IsNotNull(options.EventHubName);

                    var tokenCredential = new DefaultAzureCredential();
                    var eventHubFQDN = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                    return new EventHubConsumerClient(options.EventHubConsumerGroup, eventHubFQDN, options.EventHubName, tokenCredential, eventHubConsumerClientOptions);
                case AuthenticationType.ConnectionString:
                    EnsureArg.IsNotNull(options.ConnectionString);
                    return new EventHubConsumerClient(options.EventHubConsumerGroup, options.ConnectionString, eventHubConsumerClientOptions);
                case AuthenticationType.Custom:
                    EnsureArg.IsNotNull(options.EventHubNamespaceFQDN);
                    EnsureArg.IsNotNull(options.EventHubName);
                    EnsureArg.IsNotNull(provider);

                    var fqdn = EventHubFormatter.GetEventHubFQDN(options.EventHubNamespaceFQDN);
                    return new EventHubConsumerClient(options.EventHubConsumerGroup, fqdn, options.EventHubName, provider.GetCredential(), eventHubConsumerClientOptions);
                default:
                    throw new Exception($"Unable to create Event Hub consumer client for {options.EventHubName}: No authentication type was specified for EventHubClientOptions");
            }
        }
    }
}
