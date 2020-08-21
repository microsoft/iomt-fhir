// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class CorrelationIdNotDefinedException :
        Exception,
        ITelemetryFormattable
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

        public Metric ToMetric => new Metric(
            "CorrelationIdNotDefinedException",
            new Dictionary<string, object>
           {
                { DimensionNames.Name, "CorrelationIdNotDefinedException" },
                { DimensionNames.Category, Category.Errors },
                { DimensionNames.ErrorType, ErrorType.DeviceMessageError },
                { DimensionNames.ErrorSeverity, ErrorSeverity.Critical },
                { DimensionNames.Stage, ConnectorStage.Grouping },
           });
    }
}
