// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Events.Telemetry.Exceptions
{
    public sealed class UnauthorizedAccessEventHubException : IomtException
    {
        public UnauthorizedAccessEventHubException(
            string message,
            Exception innerException,
            string helpLink,
            string errorName)
            : base(
                  message,
                  innerException,
                  helpLink,
                  name: $"{ErrorType.EventHubError}{errorName}",
                  errorType: ErrorType.EventHubError,
                  errorSeverity: ErrorSeverity.Warning,
                  errorSource: nameof(ErrorSource.User),
                  operation: ConnectorOperation.Setup)
        {
        }
    }
}
