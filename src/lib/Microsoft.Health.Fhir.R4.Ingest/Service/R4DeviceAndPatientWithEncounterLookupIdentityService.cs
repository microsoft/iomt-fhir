// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Host;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    /// <summary>
    /// Supports looking up device, patient, and encounter ids.  Ids are cached by the device identifier in the supplied measurement group.
    /// Assumption is one patient and encounter supported per device.
    /// </summary>
    [ResourceIdentityService(ResourceIdentityServiceType.LookupWithEncounter)]
    [ResourceIdentityService(nameof(R4DeviceAndPatientWithEncounterLookupIdentityService))]
    public class R4DeviceAndPatientWithEncounterLookupIdentityService : R4DeviceAndPatientLookupIdentityService
    {
        public R4DeviceAndPatientWithEncounterLookupIdentityService(IFhirService fhirService)
           : base(fhirService)
        {
        }

        public R4DeviceAndPatientWithEncounterLookupIdentityService(IFhirService fhirService, ResourceManagementService resourceIdService)
            : base(fhirService, resourceIdService)
        {
        }

        protected async override Task<IDictionary<ResourceType, string>> ResolveResourceIdentitiesInternalAsync(IMeasurementGroup input)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            var identities = await base.ResolveResourceIdentitiesInternalAsync(input).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(input.EncounterId))
            {
                throw new ResourceIdentityNotDefinedException(ResourceType.Encounter);
            }

            var encounter = await ResourceManagementService.GetResourceByIdentityAsync<Model.Encounter>(input.EncounterId, null).ConfigureAwait(false) ?? throw new FhirResourceNotFoundException(ResourceType.Encounter);
            identities[ResourceType.Encounter] = encounter?.Id;

            return identities;
        }
    }
}
