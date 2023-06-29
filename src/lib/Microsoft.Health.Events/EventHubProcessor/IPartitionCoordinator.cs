// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public interface IPartitionCoordinator
    {
        // Registers a processorId.
        Task ResisterProcessorIdAsync(string processorId, CancellationToken cancellationToken);

        // Returns a collection of processor ids representing active processors.
        Task<IEnumerable<string>> GetActiveProcessorIdsAsync(CancellationToken cancellationToken);

        // Checks if a partition with the given is is active (has been active within the maxInactiveTime).
        Task<bool> IsPartitionActiveAsync(string partitionId, TimeSpan maxInactiveTime, CancellationToken cancellationToken);

        // Claims a partition with the given partitionId.
        Task<bool> ClaimPartitionAsync(string processorId, string partitionId, CancellationToken cancellationToken);

        // Get the partitions owned
        string[] GetOwnedPartitions();

        // Clear the partitions that are owned
        void ClearOwnedPartitions();

        // Renew ownership for a partition
        Task<bool> RenewPartitionOwnershipAsync(string processorId, string partitionId, CancellationToken cancellationToken);
    }
}
