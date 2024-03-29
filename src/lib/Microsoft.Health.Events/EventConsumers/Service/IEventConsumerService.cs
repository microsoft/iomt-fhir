﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.EventConsumers.Service
{
    public interface IEventConsumerService
    {
        Task ConsumeEvents(IEnumerable<IEventMessage> events, CancellationToken ct);

        Task ConsumeEvent(IEventMessage eventArg, CancellationToken ct);

        void NewPartitionInitialized(string partitionId);

        void PartitionProcessingStopped(string partitionId);
    }
}
