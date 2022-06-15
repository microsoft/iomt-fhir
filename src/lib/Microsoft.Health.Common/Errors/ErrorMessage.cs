// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Common.Errors
{
    public class ErrorMessage : Exception
    {
        /// <summary>
        /// A unique identifier for the ErrorMessage
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The type of error.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A string describing the error that occurred.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// The time when the error occurred.
        /// </summary>
        public DateTimeOffset ErrorTimestamp { get; set; }

        /// <summary>
        /// The message payload that led to an error to be thrown.
        /// </summary>
        /// <remarks>
        /// This property provides the input message payload so the user can determine
        /// what message was correlated with the error.
        /// </remarks>
        public JToken InputMessage { get; set; }
    }
}
