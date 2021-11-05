// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public interface IEventHubMessageService
    {
        ValueTask<EventDataBatch> CreateEventDataBatchAsync(string partitionKey);

        Task SendAsync(EventDataBatch eventData, CancellationToken token);

        Task SendAsync(IEnumerable<EventData> eventData, CancellationToken token);

        Task SendAsync(IEnumerable<EventData> eventData, string partitionKey, CancellationToken token);
    }
}
