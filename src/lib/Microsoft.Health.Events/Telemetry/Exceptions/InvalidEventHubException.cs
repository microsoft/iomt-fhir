// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Events.Telemetry.Exceptions
{
    public sealed class InvalidEventHubException : IomtTelemetryFormattableException
    {
        private static readonly string _errorType = ErrorType.EventHubError;

        public InvalidEventHubException()
        {
        }

        public InvalidEventHubException(string message)
            : base(message)
        {
        }

        public InvalidEventHubException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidEventHubException(
            string message,
            Exception innerException,
            string errorName)
            : base(
                  message,
                  innerException,
                  name: $"{_errorType}{errorName}",
                  operation: ConnectorOperation.Setup)
        {
        }

        public override string ErrType => _errorType;

        public override string ErrSeverity => ErrorSeverity.Critical;

        public override string ErrSource => nameof(ErrorSource.User);
    }
}
