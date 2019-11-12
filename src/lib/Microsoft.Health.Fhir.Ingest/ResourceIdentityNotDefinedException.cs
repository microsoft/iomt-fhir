﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class ResourceIdentityNotDefinedException :
        Exception,
        ITelemetryEvent
    {
        public ResourceIdentityNotDefinedException(ResourceType resourceType)
           : base($"Fhir resource of type {resourceType} not found.")
        {
            FhirResourceType = resourceType;
        }

        public ResourceIdentityNotDefinedException()
        {
        }

        public ResourceIdentityNotDefinedException(string message)
            : base(message)
        {
        }

        public ResourceIdentityNotDefinedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ResourceType FhirResourceType { get; private set; }

        public string EventName => $"{FhirResourceType}IdentityNotDefinedException";
    }
}
