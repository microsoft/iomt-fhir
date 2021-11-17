// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class IncompatibleDataException : IomtTelemetryFormattableException
    {
        public IncompatibleDataException()
            : base()
        {
        }

        public IncompatibleDataException(string message)
            : base(message)
        {
        }

        public IncompatibleDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string ErrName => nameof(IncompatibleDataException);

        public override string ErrType => ErrorType.DeviceMessageError;

        public override string ErrSource => nameof(ErrorSource.User);

        public override string Operation => ConnectorOperation.Normalization;
    }
}
