// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Exceptions
{
    public class CompressionNotSupportedException : IomtTelemetryFormattableException
    {
        public override string ErrName => nameof(CompressionNotSupportedException);

        public override string ErrType => ErrorType.FHIRConversionError;

        public override string ErrSeverity => ErrorSeverity.Critical;

        public override string Operation => ConnectorOperation.Grouping;
    }
}
