// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Events.Telemetry.Exceptions
{
    public sealed class UnauthorizedAccessEventHubException : IomtTelemetryFormattableException
    {
        private static readonly string _errorType = ErrorType.EventHubError;

        public UnauthorizedAccessEventHubException()
        {
        }

        public UnauthorizedAccessEventHubException(string message)
            : base(message)
        {
        }

        public UnauthorizedAccessEventHubException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UnauthorizedAccessEventHubException(
            string message,
            Exception innerException,
            string helpLink,
            string errorName)
            : base(
                  message,
                  innerException,
                  name: $"{_errorType}{errorName}",
                  operation: ConnectorOperation.Setup)
        {
            HelpLink = helpLink;
        }

        public override string ErrType => _errorType;

        public override string ErrSeverity => ErrorSeverity.Critical;

        public override string ErrSource => nameof(ErrorSource.User);
    }
}
