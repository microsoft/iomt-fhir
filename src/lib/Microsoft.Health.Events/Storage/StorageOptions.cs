// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.Storage
{
    public class StorageOptions
    {
        public const string Settings = "Storage";

        public string BlobStorageConnectionString { get; set; }

        public string BlobContainerName { get; set; }

        public string BlobPrefix { get; set; }
    }
}
