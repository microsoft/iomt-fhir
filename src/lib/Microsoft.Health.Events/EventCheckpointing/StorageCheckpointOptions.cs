// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.EventCheckpointing
{
    public class StorageCheckpointOptions
    {
        public const string Settings = "Checkpoint";

        /// <summary>
        /// Configurable prefix for the blob path where the checkpoints will be stored.
        /// The provided prefix will be appended to the app type so as to have the checkpoints individually maintained per app type.
        /// The entire blob path will comprise of this blob prefix and the event hub details(event hub namespace FQDN and event hub name)
        /// appended to it, to ensure that the checkpoints are appropriately managed if the source event hub changes.
        /// For example, for the Normalization app if the provided BlobPrefix is "devicedata", the complete blob path for
        /// the respective checkpoints will be - Normalization/devicedata/checkpoint/*eventHubNamespaceFQDN*/*eventHubName*/
        /// </summary>
        public string BlobPrefix { get; set; }

        public string CheckpointBatchCount { get; set; }
    }
}
