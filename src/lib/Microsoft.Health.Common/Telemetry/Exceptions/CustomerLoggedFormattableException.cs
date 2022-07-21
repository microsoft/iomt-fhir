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
            this.SetShouldLogToCustomer(true);
        }

        public CustomerLoggedFormattableException(string message)
            : base(message)
        {
            this.SetShouldLogToCustomer(true);
        }

        public CustomerLoggedFormattableException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.SetShouldLogToCustomer(true);
        }

        public CustomerLoggedFormattableException(
            string message,
            Exception innerException,
            string name,
            string operation)
            : base(message, innerException, name, operation)
        {
            this.SetShouldLogToCustomer(true);
        }
    }
}
