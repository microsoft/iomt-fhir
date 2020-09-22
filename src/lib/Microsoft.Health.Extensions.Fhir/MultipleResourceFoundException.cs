// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Extensions.Fhir
{
    public class MultipleResourceFoundException<T> :
        Exception,
        ITelemetryFormattable
    {
        public MultipleResourceFoundException(int resourceCount)
            : base($"Multiple resources {resourceCount} of type {typeof(T)} found, expected one")
        {
        }

        public MultipleResourceFoundException()
        {
        }

        public MultipleResourceFoundException(string message)
            : base(message)
        {
        }

        public MultipleResourceFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public string EventName => $"Multiple{typeof(T).Name}FoundException";

        public Metric ToMetric => new Metric(
            $"{EventName}",
            new Dictionary<string, object>
            {
                { DimensionNames.Name, $"{EventName}" },
                { DimensionNames.Category, Category.Errors },
                { DimensionNames.ErrorType, ErrorType.FHIRResourceError },
                { DimensionNames.ErrorSeverity, ErrorSeverity.Warning },
                { DimensionNames.Operation, ConnectorOperation.FHIRConversion },
            });
    }
}
