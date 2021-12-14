// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.Telemetry
{
    public enum EventHubErrorCode
    {
        /// <summary>
        /// Error code that indicates failures in initializing event hub partition
        /// </summary>
        EventHubPartitionInitFailed,

        /// <summary>
        /// Error code that categorizes invalid configurations (e.g. invalid namespace/FQDN, event hub name, or consumer group)
        /// </summary>
        ConfigurationError,

        /// <summary>
        /// Error code that categorizes authentication errors (eg: exceptions of the type UnauthorizedAccessException)
        /// </summary>
        AuthorizationError,

        /// <summary>
        /// Error code that categorizes all other generic Exceptions
        /// </summary>
        GeneralError,

        /// <summary>
        /// Error code that categorizes general invalid operation exceptions (eg: exceptions encountered of type InvalidOperationException)
        /// </summary>
        InvalidOperationError,

        /// <summary>
        /// Error code that categorizes general request failed exceptions (eg: exceptions encountered of type RequestFailedException)
        /// </summary>
        RequestFailedError,
    }
}
