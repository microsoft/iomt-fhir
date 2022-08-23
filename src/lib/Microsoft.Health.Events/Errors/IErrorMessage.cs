// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Events.Errors
{
    public interface IErrorMessage
    {
        /// <summary>
        /// A unique identifier for the ErrorMessage
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The type of error.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// A string describing the error that occurred.
        /// </summary>
        string Details { get; set; }

        /// <summary>
        /// The time when the error occurred.
        /// </summary>
        DateTimeOffset ErrorTimestamp { get; set; }

        /// <summary>
        /// Optional properties that provide additional context for the Error
        /// </summary>
        IDictionary<string, object> Values { get; }
    }
}
