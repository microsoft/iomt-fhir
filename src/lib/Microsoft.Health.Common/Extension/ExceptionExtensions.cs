// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Common.Extension
{
    public static class ExceptionExtensions
    {
        private const string LogToCustomerAttribute = "LogToCustomer";

        /// <summary>
        /// Flags the exception as being logged to the customer or not.
        /// </summary>
        /// <param name="error">The root exception</param>
        /// <param name="logToCustomer">Boolean to set whether the exception should be logged to third-party customers</param>
        public static void SetShouldLogToCustomer(this Exception error, bool logToCustomer)
        {
            error.Data[LogToCustomerAttribute] = logToCustomer;
        }

        /// <summary>
        /// Returns true iff the exception is flagged as being logged to the customer.
        /// </summary>
        /// <param name="error">The root exception</param>
        /// <returns>True iff the exception should be logged to third-party customers</returns>
        public static bool ShouldLogToCustomer(this Exception error)
        {
            return error.Data[LogToCustomerAttribute] as bool? ?? false;
        }
    }
}
