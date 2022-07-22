// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Common.Extension
{
    public static class ExceptionExtensions
    {
        private const string LogForwardingAttribute = "ShouldForwardLog";

        /// <summary>
        /// Flags the exception as being logged to the customer or not.
        /// </summary>
        /// <param name="error">The root exception</param>
        /// <param name="shouldForwardLog">Boolean to set whether the exception should be logged to third-party</param>
        public static Exception SetLogForwarding(this Exception error, bool shouldForwardLog)
        {
            error.Data[LogForwardingAttribute] = shouldForwardLog;
            return error;
        }

        /// <summary>
        /// Returns true iff the exception is flagged as being logged to the customer.
        /// </summary>
        /// <param name="error">The root exception</param>
        /// <returns>True iff the exception should be logged to third-party</returns>
        public static bool IsLogForwardingEnabled(this Exception error)
        {
            return error.Data[LogForwardingAttribute] as bool? ?? false;
        }
    }
}
