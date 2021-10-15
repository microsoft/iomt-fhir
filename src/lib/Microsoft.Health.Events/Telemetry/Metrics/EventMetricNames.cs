// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.Telemetry
{
    public enum EventMetricNames
    {
        /// <summary>
        /// Signals that the event hub was changed.
        /// </summary>
        EventHubChanged,

        /// <summary>
        /// Signals that an event hub partition has been intialized.
        /// </summary>
        EventHubPartitionInitialized,

        /// <summary>
        /// Signals that a batch of event hub events was created.
        /// </summary>
        EventBatchCreated,

        /// <summary>
        /// Signals that a batch of event hub events was flushed.
        /// </summary>
        EventsFlushed,

        /// <summary>
        /// Signals that a new watermark was published for a partition.
        /// </summary>
        EventsWatermarkUpdated,

        /// <summary>
        /// Signals the timestamp corresponding to the last processed event per partition.
        /// </summary>
        EventTimestampLastProcessedPerPartition,
    }
}
