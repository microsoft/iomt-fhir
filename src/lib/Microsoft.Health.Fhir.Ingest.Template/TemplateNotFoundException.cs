// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateNotFoundException :
        Exception,
        ITelemetryFormattable
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

        public Metric ToMetric => nameof(TemplateNotFoundException).ToErrorMetric(ConnectorOperation.FHIRConversion, ErrorType.GeneralError, ErrorSeverity.Critical);
    }
}
