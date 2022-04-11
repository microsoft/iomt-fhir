// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class InvalidQuantityFhirValueException : IomtTelemetryFormattableException
    {
        public InvalidQuantityFhirValueException()
            : base()
        {
        }

        public InvalidQuantityFhirValueException(string message)
            : base(message)
        {
        }

        public InvalidQuantityFhirValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string ErrName => nameof(InvalidQuantityFhirValueException);

        public override string ErrType => ErrorType.FHIRResourceError;

        public override string Operation => ConnectorOperation.FHIRConversion;
    }
}
