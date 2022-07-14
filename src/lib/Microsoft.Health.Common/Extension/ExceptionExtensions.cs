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

        public static void SetLogToCustomer(this Exception error, bool logToCustomer)
        {
            error.Data[LogToCustomerAttribute] = logToCustomer;
        }

        public static bool LogToCustomer(this Exception error)
        {
            return error.Data[LogToCustomerAttribute] as bool? ?? false;
        }
    }
}
