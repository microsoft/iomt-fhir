// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Core;

namespace Microsoft.Health.Events.Common
{
    public enum AuthenticationType
    {
        /// <summary>A managed identity is used to connect to the Event Hub.</summary>
        ManagedIdentity,

        /// <summary>A connection string is used to connect to the Event Hub.</summary>
        ConnectionString,

        /// <summary>A custom authentication method is used to connect to the Event Hub.</summary>
        Custom,
    }

    public class EventHubClientOptions
    {
        public const string Settings = "Settings";

        public AuthenticationType AuthenticationType { get; set; }

        public string EventHubNamespaceFQDN { get; set; }

        public string EventHubConsumerGroup { get; set; }

        public string EventHubName { get; set; }

        public string ConnectionString { get; set; }

        public TokenCredential EventHubTokenCredential { get; set; }

        public Uri StorageCheckpointUri { get; set; }
    }
}
