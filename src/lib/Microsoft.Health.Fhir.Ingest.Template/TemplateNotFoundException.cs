// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateNotFoundException :
        Exception,
        ITelemetryFormattable
    {
        private static Metric _templateNotFound = new Metric(
            "TemplateNotFoundException",
            new Dictionary<string, object>
            {
                { DimensionNames.Name, "TemplateNotFoundException" },
                { DimensionNames.Category, Category.Errors },
                { DimensionNames.ErrorType, ErrorType.GeneralError },
                { DimensionNames.ErrorSeverity, ErrorSeverity.Critical },
                { DimensionNames.Operation, ConnectorOperation.Unknown },
            });

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

        public Metric ToMetric => _templateNotFound;
    }
}
