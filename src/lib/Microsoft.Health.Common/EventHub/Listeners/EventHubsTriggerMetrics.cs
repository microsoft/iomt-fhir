// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Azure.WebJobs.Host.Scale;

namespace Microsoft.Health.Common.EventHubs.Listeners
{
    public class EventHubsTriggerMetrics : ScaleMetrics
    {
        /// <summary>
        /// The total number of unprocessed events across all partitions.
        /// </summary>
        public long EventCount { get; set; }

        /// <summary>
        /// The number of partitions.
        /// </summary>
        public int PartitionCount { get; set; }
    }
}
