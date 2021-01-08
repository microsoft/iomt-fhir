// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public abstract class FhirImportService : IFhirImportService<IMeasurementGroup, ILookupTemplate<IFhirTemplate>>
    {
        public const string ServiceSystem = @"https://azure.microsoft.com/en-us/services/iomt-fhir-connector/";

        public abstract Task ProcessAsync(ILookupTemplate<IFhirTemplate> config, IMeasurementGroup data, Func<Exception, IMeasurementGroup, Task<bool>> errorConsumer = null);

        protected static (string Identifer, string System) GenerateObservationId(IObservationGroup observationGroup, string deviceId, string patientId)
        {
            EnsureArg.IsNotNull(observationGroup, nameof(observationGroup));
            EnsureArg.IsNotNullOrWhiteSpace(deviceId, nameof(deviceId));
            EnsureArg.IsNotNullOrWhiteSpace(patientId, nameof(patientId));

            var value = $"{patientId}.{deviceId}.{observationGroup.Name}.{observationGroup.GetIdSegment()}";

            return (value, ServiceSystem);
        }
    }
}
