// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Extension;

namespace Microsoft.Health.Common.Telemetry.Exceptions
{
    public class CustomerLoggedFormattableException : IomtTelemetryFormattableException
    {
        public CustomerLoggedFormattableException()
        {
            this.SetLogToCustomer(true);
        }

        public CustomerLoggedFormattableException(string message)
            : base(message)
        {
            this.SetLogToCustomer(true);
        }

        public CustomerLoggedFormattableException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.SetLogToCustomer(true);
        }

        public CustomerLoggedFormattableException(
            string message,
            Exception innerException,
            string name,
            string operation)
            : base(message, innerException, name, operation)
        {
            this.SetLogToCustomer(true);
        }
    }
}
