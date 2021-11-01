// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.Telemetry
{
    public enum EventHubErrorCode
    {
        /// <summary>
        /// Error code for an issue relating to an Event Hub (e.g. invalid name or consumer group)
        /// </summary>
        InstanceError,

        /// <summary>
        /// Error code for an issue relating to an Event Hubs Namespace (e.g. invalid FQDN)
        /// </summary>
        NamespaceError,

        /// <summary>
        /// Error code that indicates failures in initializing event hub partition
        /// </summary>
        EventHubPartitionInitFailed,

        /// <summary>
        /// Error code that categorizes authentication errors (eg: exceptions of the type UnauthorizedAccessException)
        /// </summary>
        AuthorizationError,

        /// <summary>
        /// Error code that categorizes all other generic Exceptions
        /// </summary>
        GeneralError,
    }
}
