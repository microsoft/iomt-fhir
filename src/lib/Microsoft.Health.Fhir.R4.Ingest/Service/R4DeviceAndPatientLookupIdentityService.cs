﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Repository;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Host;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    [ResourceIdentityService(ResourceIdentityServiceType.Lookup)]
    [ResourceIdentityService(nameof(R4DeviceAndPatientLookupIdentityService))]
    public class R4DeviceAndPatientLookupIdentityService : DeviceAndPatientLookupIdentityService
    {
        private readonly IFhirServiceRepository _fhirServiceRepository;
        private readonly ResourceManagementService _resourceManagementService;

        public R4DeviceAndPatientLookupIdentityService(IFhirServiceRepository fhirServiceRepository)
            : this(fhirServiceRepository, new ResourceManagementService(fhirServiceRepository))
        {
        }

        public R4DeviceAndPatientLookupIdentityService(IFhirServiceRepository fhirServiceRepository, ResourceManagementService resourceManagementService)
        {
            _fhirServiceRepository = EnsureArg.IsNotNull(fhirServiceRepository, nameof(fhirServiceRepository));
            _resourceManagementService = EnsureArg.IsNotNull(resourceManagementService, nameof(resourceManagementService));
        }

        protected IFhirServiceRepository FhirServiceRepository => _fhirServiceRepository;

        protected ResourceManagementService ResourceManagementService => _resourceManagementService;

        protected static string GetPatientIdFromDevice(Model.Device device)
        {
            EnsureArg.IsNotNull(device, nameof(device));

            return device.Patient?.GetId<Model.Patient>() ?? throw new FhirResourceNotFoundException(ResourceType.Patient);
        }

        protected async override Task<(string DeviceId, string PatientId)> LookUpDeviceAndPatientIdAsync(string value, string system = null)
        {
            var device = await ResourceManagementService.GetResourceByIdentityAsync<Model.Device>(value, system).ConfigureAwait(false) ?? throw new FhirResourceNotFoundException(ResourceType.Device);
            return (device.Id, GetPatientIdFromDevice(device));
        }
    }
}