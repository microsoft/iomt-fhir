// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Common.Storage
{
    public enum AuthenticationType
    {
        /// <summary>A managed identity is used to connect to Storage.</summary>
        ManagedIdentity,

        /// <summary>A connection string is used to connect to Storage.</summary>
        ConnectionString,

        /// <summary>A custom authentication method is used to connect to Storage.</summary>
        Custom,
    }

    public class BlobContainerClientOptions
    {
        public AuthenticationType AuthenticationType { get; set; }

        public Uri BlobStorageContainerUri { get; set; }

        public string ConnectionString { get; set; }
    }
}
