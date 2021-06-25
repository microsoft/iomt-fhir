// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.EventCheckpointing
{
    public interface ICheckpointClient
    {
        Task SetCheckpointAsync(IEventMessage eventArg);

        Task PublishCheckpointAsync(string partitionId);

        Task<Checkpoint> GetCheckpointForPartitionAsync(string partitionId);

        Task ResetCheckpointsAsync();
    }
}
