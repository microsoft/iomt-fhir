// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Events.Telemetry.Exceptions
{
    public class UnclaimedPartitionException : IomtTelemetryFormattableException
    {
        private static readonly string _errorType = ErrorType.EventHubError;

        public UnclaimedPartitionException(
            string message)
            : base(message)
        {
        }

        public override string ErrType => _errorType;

        public override string ErrSeverity => ErrorSeverity.Warning;

        public override string ErrSource => nameof(ErrorSource.Service);
    }
}
