// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public interface IEventProcessingMeter
    {
        Task<EventStats> CalculateEventStats(IEnumerable<EventData> events);
    }
}
