// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Host;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    [ResourceIdentityService(ResourceIdentityServiceType.Create)]
    [ResourceIdentityService(nameof(R4DeviceAndPatientCreateIdentityService))]
    public class R4DeviceAndPatientCreateIdentityService : R4DeviceAndPatientLookupIdentityService
    {
        public R4DeviceAndPatientCreateIdentityService(IFhirService fhirService)
            : base(fhirService)
        {
        }

        public R4DeviceAndPatientCreateIdentityService(IFhirService fhirService, ResourceManagementService resourceIdService)
            : base(fhirService, resourceIdService)
        {
        }

        protected async override Task<IDictionary<ResourceType, string>> ResolveResourceIdentitiesInternalAsync(IMeasurementGroup input)
        {
            try
            {
                return await base.ResolveResourceIdentitiesInternalAsync(input).ConfigureAwait(false);
            }
            catch (FhirResourceNotFoundException ex)
            {
                // Continue with create path if device  or patient wasn't found.
                if (!(ex.FhirResourceType == ResourceType.Device || ex.FhirResourceType == ResourceType.Patient))
                {
                    throw;
                }
            }

            var (deviceId, patientId) = await EnsureDeviceAndPatientExistsAsync(input).ConfigureAwait(false);
            return CreateIdentityLookup(deviceId, patientId);
        }

        /// <summary>
        /// Ensures a patient and device resource exists and returns the relevant internal ids.
        /// </summary>
        /// <param name="input">IMeasurementGroup to retrieve device and patient identifiers from.</param>
        /// <returns>Internal reference id to the patient and device resources found or created.</returns>
        /// <exception cref="PatientIdentityNotDefinedException">Thrown when a unique patient identifier isn't found in the provided input.</exception>
        /// <exception cref="PatientDeviceMismatchException">Thrown when expected patient internal id of the device doesn't match the actual patient internal id.</exception>
        protected async virtual Task<(string DeviceId, string PatientId)> EnsureDeviceAndPatientExistsAsync(IMeasurementGroup input)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            // Verify one unique patient identity is present in the measurement group

            if (string.IsNullOrWhiteSpace(input.PatientId))
            {
                throw new ResourceIdentityNotDefinedException(ResourceType.Patient);
            }

            // Begin critical section

            var patient = await ResourceManagementService.EnsureResourceByIdentityAsync<Model.Patient>(
                input.PatientId,
                null,
                (p, id) => p.Identifier = new List<Model.Identifier> { id })
                .ConfigureAwait(false);

            var device = await ResourceManagementService.EnsureResourceByIdentityAsync<Model.Device>(
                GetDeviceIdentity(input),
                ResourceIdentityOptions?.DefaultDeviceIdentifierSystem,
                (d, id) =>
                {
                    d.Identifier = new List<Model.Identifier> { id };
                    d.Patient = patient.ToReference();
                })
                .ConfigureAwait(false);

            patient.ToReference();

            if (device.Patient == null)
            {
                device.Patient = patient.ToReference();
                device = await FhirService.UpdateResourceAsync(device).ConfigureAwait(false);
            }
            else if (device.Patient.GetId<Model.Patient>() != patient.Id)
            {
                // Device is linked to a different patient.  Current behavior is undefined, throw an exception.
                throw new PatientDeviceMismatchException();
            }

            // End critical section

            return (device.Id, patient.Id);
        }
    }
}