// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Extension;

namespace Microsoft.Health.Common.Telemetry.Exceptions
{
    public class ThirdPartyLoggedFormattableException : IomtTelemetryFormattableException
    {
        public ThirdPartyLoggedFormattableException()
        {
            this.SetLogForwarding(true);
        }

        public ThirdPartyLoggedFormattableException(string message)
            : base(message)
        {
            this.SetLogForwarding(true);
        }

        public ThirdPartyLoggedFormattableException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.SetLogForwarding(true);
        }

        public ThirdPartyLoggedFormattableException(
            string message,
            Exception innerException,
            string name,
            string operation)
            : base(message, innerException, name, operation)
        {
            this.SetLogForwarding(true);
        }
    }
}
