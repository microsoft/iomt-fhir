// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class CorrelationIdNotDefinedException : IomtTelemetryFormattableException
    {
        public CorrelationIdNotDefinedException(string message)
            : base(message)
        {
        }

        public CorrelationIdNotDefinedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CorrelationIdNotDefinedException()
        {
        }

        public override string ErrName => nameof(CorrelationIdNotDefinedException);

        public override string ErrType => ErrorType.DeviceMessageError;

        public override string ErrSeverity => ErrorSeverity.Critical;

        public override string Operation => ConnectorOperation.Grouping;
    }
}
