// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Data;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class DeviceAndPatientLookupIdentityServiceTests
    {
        [Fact]
        public async void GivenFoundDeviceAndPatientId_WhenResolveResourceIdentitiesAsync_ThenCorrectDictionaryReturned_Test()
        {
            var idSrv = Substitute.ForPartsOf<TestHarnessDeviceAndPatientLookupIdentityService>();
            idSrv.HarnessLookUpDeviceAndPatientIdAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(("did", "pid")));
            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");

            var result = await idSrv.ResolveResourceIdentitiesAsync(mg);
            Assert.NotNull(result);
            Assert.Equal(Enum.GetValues(typeof(ResourceType)).Length, result.Count);
            Assert.Equal("did", result[ResourceType.Device]);
            Assert.Equal("pid", result[ResourceType.Patient]);
            Assert.Null(result[ResourceType.Encounter]);

            await idSrv.Received(1).HarnessLookUpDeviceAndPatientIdAsync("deviceId", null);
        }

        public abstract class TestHarnessDeviceAndPatientLookupIdentityService : DeviceAndPatientLookupIdentityService
        {
            public abstract Task<(string DeviceId, string PatientId)> HarnessLookUpDeviceAndPatientIdAsync(string value, string system);

            protected async override Task<(string DeviceId, string PatientId)> LookUpDeviceAndPatientIdAsync(string value, string system = null)
            {
                return await HarnessLookUpDeviceAndPatientIdAsync(value, system).ConfigureAwait(false);
            }
        }
    }
}