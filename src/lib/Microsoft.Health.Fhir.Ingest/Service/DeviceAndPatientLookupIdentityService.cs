﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public abstract class DeviceAndPatientLookupIdentityService : CachedResourceIdentityService
    {
        protected override string GetCacheKey(IMeasurementGroup input)
        {
            return GetDeviceIdentity(input);
        }

        protected async override Task<IDictionary<ResourceType, string>> ResolveResourceIdentitiesInternalAsync(IMeasurementGroup input)
        {
            var system = ResourceIdentityOptions?.DefaultDeviceIdentifierSystem;
            var (deviceId, patientId) = await LookUpDeviceAndPatientIdAsync(GetDeviceIdentity(input), system).ConfigureAwait(false);
            return CreateIdentityLookup(deviceId, patientId);
        }

        protected static string GetDeviceIdentity(IMeasurementGroup input)
        {
            return input.DeviceId;
        }

        protected static IDictionary<ResourceType, string> CreateIdentityLookup(string deviceId, string patientId)
        {
            var lookup = IdentityLookupFactory.Instance.Create();
            lookup[ResourceType.Device] = deviceId;
            lookup[ResourceType.Patient] = patientId;
            return lookup;
        }

        protected abstract Task<(string DeviceId, string PatientId)> LookUpDeviceAndPatientIdAsync(string value, string system = null);
    }
}