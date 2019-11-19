// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Fhir.Ingest.Data;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4DeviceAndPatientLookupIdentityService : DeviceAndPatientLookupIdentityService
    {
        private readonly IFhirClient _fhirClient;
        private readonly ResourceManagementService _resourceManagementService = null;

        public R4DeviceAndPatientLookupIdentityService(IFhirClient fhirClient)
            : this(fhirClient, new ResourceManagementService())
        {
        }

        public R4DeviceAndPatientLookupIdentityService(IFhirClient fhirClient, ResourceManagementService resourceManagementService)
        {
            _fhirClient = EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));
            _resourceManagementService = EnsureArg.IsNotNull(resourceManagementService, nameof(resourceManagementService));
        }

        protected IFhirClient FhirClient => _fhirClient;

        protected ResourceManagementService ResourceManagementService => _resourceManagementService;

        protected async override Task<(string DeviceId, string PatientId)> LookUpDeviceAndPatientIdAsync(string value, string system = null)
        {
            var device = await ResourceManagementService.GetResourceByIdentityAsync<Model.Device>(FhirClient, value, system).ConfigureAwait(false) ?? throw new FhirResourceNotFoundException(ResourceType.Device);
            return (device.Id, GetPatientIdFromDevice(device));
        }

        protected static string GetPatientIdFromDevice(Model.Device device)
        {
            EnsureArg.IsNotNull(device, nameof(device));

            return device.Patient?.GetId<Model.Patient>() ?? throw new FhirResourceNotFoundException(ResourceType.Patient);
        }
    }
}
