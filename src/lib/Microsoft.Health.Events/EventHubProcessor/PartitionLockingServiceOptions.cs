// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Auth;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class PartitionLockingServiceOptions
    {
        public const string Settings = "PartitionLocking";

        public Uri BlobContainerUri { get; set; }

        public IAzureCredentialProvider StorageTokenCredential { get; set; }

        public bool Enabled { get; set; } = false;
    }
}
