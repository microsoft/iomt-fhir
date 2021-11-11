// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateNotFoundException : IomtTelemetryFormattableException
    {
        public TemplateNotFoundException(string message)
            : base(message)
        {
        }

        public TemplateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TemplateNotFoundException()
        {
        }

        public override string ErrName => nameof(TemplateNotFoundException);

        public override string ErrType => ErrorType.GeneralError;

        public override string ErrSeverity => ErrorSeverity.Critical;

        public override string Operation => ConnectorOperation.FHIRConversion;
    }
}
