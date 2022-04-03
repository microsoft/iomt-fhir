// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.EventCheckpointing
{
    public interface ICheckpointClient
    {
        Task SetCheckpointAsync(IEventMessage eventArg, IEnumerable<KeyValuePair<Metric, double>> metrics = null);

        Task<Checkpoint> GetCheckpointForPartitionAsync(string partitionId, CancellationToken cancellationToken);

        Task ResetCheckpointsAsync(CancellationToken cancellationToken = default);
    }
}
