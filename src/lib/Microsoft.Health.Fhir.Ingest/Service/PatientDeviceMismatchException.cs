// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class PatientDeviceMismatchException :
        Exception,
        ITelemetryFormattable
    {
        public PatientDeviceMismatchException(string message)
            : base(message)
        {
        }

        public PatientDeviceMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PatientDeviceMismatchException()
        {
        }

        public Metric ToMetric => new Metric(
            "PatientDeviceMismatchException",
            new Dictionary<string, object>
            {
                { DimensionNames.Name, "PatientDeviceMismatchException" },
                { DimensionNames.Category, Category.Errors },
                { DimensionNames.ErrorType, ErrorType.FHIRResourceError },
                { DimensionNames.ErrorSeverity, ErrorSeverity.Warning },
                { DimensionNames.Operation, ConnectorOperation.FHIRConversion },
            });
    }
}
