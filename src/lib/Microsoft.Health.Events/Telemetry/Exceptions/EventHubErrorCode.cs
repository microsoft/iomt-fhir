// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.Telemetry
{
    public enum EventHubErrorCode
    {
        /// <summary>
        /// Error code that categorizes exceptions of the type EventHubsException
        /// </summary>
        OperationError,

        /// <summary>
        /// Error code that indicates failures in initializing event hub partition
        /// </summary>
        EventHubPartitionInitFailed,

        /// <summary>
        /// Error code that categorizes exceptions of the type SocketException
        /// </summary>
        SocketError,

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
