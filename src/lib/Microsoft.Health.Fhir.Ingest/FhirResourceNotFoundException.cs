// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest
{
    public class FhirResourceNotFoundException : IomtTelemetryFormattableException
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

        public override string ErrName => $"{FhirResourceType}NotFoundException";

        public override string ErrType => ErrorType.FHIRResourceError;

        public override string Operation => ConnectorOperation.FHIRConversion;
    }
}
