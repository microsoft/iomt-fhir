// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs.Producer;

namespace Microsoft.Health.Events.EventProducers
{
    public interface IEventProducerClientFactory
    {
        EventHubProducerClient GetEventHubProducerClient(EventProducerClientOptions options);
    }
}
