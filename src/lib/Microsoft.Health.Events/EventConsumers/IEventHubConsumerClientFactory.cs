// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;

namespace Microsoft.Health.Events.EventConsumers
{
    public interface IEventHubConsumerClientFactory
    {
        EventHubConsumerClient CreateConsumerClient(EventHubClientOptions options, EventHubConsumerClientOptions eventHubConsumerClientOptions, IAzureCredentialProvider provider = null);
    }
}
