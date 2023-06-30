// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class ProcessorCountException : IomtTelemetryFormattableException
    {
        public ProcessorCountException(string message)
            : base(message)
        {
        }

        public override string ErrName => nameof(ProcessorCountException);

        public override string ErrType => ErrorType.ServiceInformation;

        public override string ErrSeverity => ErrorSeverity.Critical;

        public override string Operation => ConnectorOperation.Unknown;
    }
}
