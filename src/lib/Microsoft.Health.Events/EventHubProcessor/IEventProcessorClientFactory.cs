﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using Microsoft.Health.Common.Auth;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public interface IEventProcessorClientFactory
    {
        EventProcessorClient CreateProcessorClient(BlobContainerClient blobContainerClient, EventProcessorClientFactoryOptions options, EventProcessorClientOptions eventProcessorClientOptions);

        EventProcessorClient CreateProcessorClient(IAzureCredentialProvider provider, BlobContainerClient blobContainerClient, EventProcessorClientFactoryOptions options, EventProcessorClientOptions eventProcessorClientOptions);
    }
}