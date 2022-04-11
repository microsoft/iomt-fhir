// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Common.Telemetry.Exceptions
{
    public class ManagedIdentityAuthenticationError : IomtTelemetryFormattableException
    {
        public ManagedIdentityAuthenticationError(
            string message,
            Exception innerException,
            string errorName)
            : base(
                  message,
                  innerException,
                  name: $"{nameof(ManagedIdentityAuthenticationError)}{errorName}",
                  operation: ConnectorOperation.Setup)
        {
        }

        public override string ErrType => ErrorType.AuthenticationError;

        public override string ErrSeverity => ErrorSeverity.Critical;

        public override string ErrSource => nameof(ErrorSource.User);
    }
}
