// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs.Producer;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;

namespace Microsoft.Health.Events.EventProducers
{
    public interface IEventProducerClientFactory
    {
        EventHubProducerClient GetEventHubProducerClient(EventHubClientOptions options, IAzureCredentialProvider provider = null);
    }
}
