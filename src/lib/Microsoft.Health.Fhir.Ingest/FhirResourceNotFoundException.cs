// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest
{
    public class FhirResourceNotFoundException :
        Exception,
        ITelemetryFormattable
    {
        public FhirResourceNotFoundException(ResourceType resourceType)
           : base($"Fhir resource of type {resourceType} not found.")
        {
            FhirResourceType = resourceType;
        }

        public FhirResourceNotFoundException()
        {
        }

        public FhirResourceNotFoundException(string message)
            : base(message)
        {
        }

        public FhirResourceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ResourceType FhirResourceType { get; private set; }

        public string EventName => $"{FhirResourceType}NotFoundException";

        public Metric ToMetric => EventName.ToErrorMetric(ConnectorOperation.FHIRConversion, ErrorType.FHIRResourceError, ErrorSeverity.Warning);
    }
}
