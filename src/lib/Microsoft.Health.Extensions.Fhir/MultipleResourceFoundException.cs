// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
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

        public Metric ToMetric => EventName.ToErrorMetric(ConnectorOperation.FHIRConversion, ErrorType.FHIRResourceError, ErrorSeverity.Warning);
    }
}
