// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    /// <summary>
    /// Provide a testable interface for EventHubClient operations.
    /// </summary>
    public interface IEventHubService
    {
        Task SendAsync(EventData eventData);

        Task SendAsync(EventData eventData, string partitionKey);

        Task SendAsync(IEnumerable<EventData> eventData, string partitionKey);

        Task CloseAsync();
    }
}
