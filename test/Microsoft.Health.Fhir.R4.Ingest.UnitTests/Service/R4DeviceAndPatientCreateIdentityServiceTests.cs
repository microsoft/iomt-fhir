// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Health.Common;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using NSubstitute;
using Xunit;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4DeviceAndPatientCreateIdentityServiceTests
    {
        [Fact]
        public async void GivenValidDeviceIdentifier_WhenResolveResourceIdentitiesAsync_ThenDeviceAndPatientIdReturnedAndCreateNotInvoked_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            var resourceService = Substitute.For<ResourceManagementService>(fhirClient);
            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Patient/123"),
            };

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(device));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal("1", ids[ResourceType.Device]);
                Assert.Equal("123", ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
            await resourceService.DidNotReceiveWithAnyArgs().EnsureResourceByIdentityAsync<Model.Device>(null, null, null);
            await resourceService.DidNotReceiveWithAnyArgs().EnsureResourceByIdentityAsync<Model.Patient>(null, null, null);
        }

        [Fact]
        public async void GivenValidDeviceIdentifierWithSystem_WhenResolveResourceIdentitiesAsync_ThenDeviceAndPatientIdReturnedAndCreateNotInvokedWithSystemUsed_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            var resourceService = Substitute.For<ResourceManagementService>(fhirClient);
            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Patient/123"),
            };

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(device));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                idSrv.Initialize(new ResourceIdentityOptions { DefaultDeviceIdentifierSystem = "mySystem" });
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal("1", ids[ResourceType.Device]);
                Assert.Equal("123", ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", "mySystem");
            await resourceService.DidNotReceiveWithAnyArgs().EnsureResourceByIdentityAsync<Model.Device>(null, null, null);
            await resourceService.DidNotReceiveWithAnyArgs().EnsureResourceByIdentityAsync<Model.Patient>(null, null, null);
        }

        [Fact]
        public async void GivenPatientNotFoundException_WhenResolveResourceIdentitiesAsync_ThenDeviceAndPatientCreateInvokedAndIdsReturned_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService()
                .SearchReturnsEmptyBundle()
                .UpdateReturnsResource<Model.Patient>()
                .CreateReturnsResource<Model.Patient>()
                .UpdateReturnsResource<Model.Device>()
                .CreateReturnsResource<Model.Device>();

            var deviceIdentifer = new Model.Identifier { Value = "deviceId" };
            var patientIdentifer = new Model.Identifier { Value = "patientId" };

            var resourceService = Substitute.ForPartsOf<ResourceManagementService>(fhirClient);
            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns(deviceIdentifer.Value);
            mg.PatientId.Returns(patientIdentifer.Value);

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<Model.Device>(new FhirResourceNotFoundException(ResourceType.Patient)));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal(deviceIdentifer.ComputeHashForIdentifier(), ids[ResourceType.Device]);
                Assert.Equal(patientIdentifer.ComputeHashForIdentifier(), ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>(deviceIdentifer.Value, null);
            await resourceService.Received(1).EnsureResourceByIdentityAsync(deviceIdentifer.Value, null, Arg.Any<Action<Model.Device, Model.Identifier>>());
            await resourceService.Received(1).EnsureResourceByIdentityAsync(patientIdentifer.Value, null, Arg.Any<Action<Model.Patient, Model.Identifier>>());
        }

        [Fact]
        public async void GivenDeviceNotFoundException_WhenResolveResourceIdentitiesAsync_ThenDeviceAndPatientCreateInvokedAndIdsReturned_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService()
               .SearchReturnsEmptyBundle()
               .UpdateReturnsResource<Model.Patient>()
               .CreateReturnsResource<Model.Patient>()
               .UpdateReturnsResource<Model.Device>()
               .CreateReturnsResource<Model.Device>();

            var deviceIdentifer = new Model.Identifier { Value = "deviceId" };
            var patientIdentifer = new Model.Identifier { Value = "patientId" };

            var resourceService = Substitute.ForPartsOf<ResourceManagementService>(fhirClient);

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns(deviceIdentifer.Value);
            mg.PatientId.Returns(patientIdentifer.Value);

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<Model.Device>(new FhirResourceNotFoundException(ResourceType.Device)));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal(deviceIdentifer.ComputeHashForIdentifier(), ids[ResourceType.Device]);
                Assert.Equal(patientIdentifer.ComputeHashForIdentifier(), ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>(deviceIdentifer.Value, null);
            await resourceService.Received(1).EnsureResourceByIdentityAsync(deviceIdentifer.Value, null, Arg.Any<Action<Model.Device, Model.Identifier>>());
            await resourceService.Received(1).EnsureResourceByIdentityAsync(patientIdentifer.Value, null, Arg.Any<Action<Model.Patient, Model.Identifier>>());
        }

        [Fact]
        public async void GivenDeviceNotFoundExceptionWithDeviceSystemSet_WhenResolveResourceIdentitiesAsync_ThenDeviceAndPatientCreateInvokedWithDeviceSystemAndIdsReturned_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService()
               .SearchReturnsEmptyBundle()
               .UpdateReturnsResource<Model.Patient>()
               .CreateReturnsResource<Model.Patient>()
               .UpdateReturnsResource<Model.Device>()
               .CreateReturnsResource<Model.Device>();

            var deviceIdentifer = new Model.Identifier { Value = "deviceId", System = "mySystem" };
            var patientIdentifer = new Model.Identifier { Value = "patientId" };

            var resourceService = Substitute.ForPartsOf<ResourceManagementService>(fhirClient);

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns(deviceIdentifer.Value);
            mg.PatientId.Returns(patientIdentifer.Value);

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<Model.Device>(new FhirResourceNotFoundException(ResourceType.Device)));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                idSrv.Initialize(new ResourceIdentityOptions { DefaultDeviceIdentifierSystem = "mySystem" });
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal(deviceIdentifer.ComputeHashForIdentifier(), ids[ResourceType.Device]);
                Assert.Equal(patientIdentifer.ComputeHashForIdentifier(), ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>(deviceIdentifer.Value, deviceIdentifer.System);
            await resourceService.Received(1).EnsureResourceByIdentityAsync(deviceIdentifer.Value, deviceIdentifer.System, Arg.Any<Action<Model.Device, Model.Identifier>>());
            await resourceService.Received(1).EnsureResourceByIdentityAsync(patientIdentifer.Value, null, Arg.Any<Action<Model.Patient, Model.Identifier>>());
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData("")]
        [InlineData(" ")]
        public async void GivenIdNotFoundExceptionWithNoPatientId_WhenResolveResourceIdentitiesAsync_ThenResourceIdentityNotDefinedExceptionThrown_Test(string value)
        {
            var fhirClient = Utilities.CreateMockFhirService();
            var resourceService = Substitute.For<ResourceManagementService>(fhirClient);

            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Patient/123"),
            };

            var patient = new Model.Patient
            {
                Id = "123",
            };

            var createService = Substitute.For<ResourceManagementService>(fhirClient);
            resourceService.EnsureResourceByIdentityAsync<Model.Device>(null, null, null).ReturnsForAnyArgs(Task.FromResult(device));
            resourceService.EnsureResourceByIdentityAsync<Model.Patient>(null, null, null).ReturnsForAnyArgs(Task.FromResult(patient));

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");
            mg.PatientId.Returns(value);

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<Model.Device>(new FhirResourceNotFoundException(ResourceType.Patient)));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                var ex = await Assert.ThrowsAsync<ResourceIdentityNotDefinedException>(() => idSrv.ResolveResourceIdentitiesAsync(mg));
                Assert.Equal(ResourceType.Patient, ex.FhirResourceType);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
            await resourceService.DidNotReceiveWithAnyArgs().EnsureResourceByIdentityAsync<Model.Device>(null, null, null);
            await resourceService.DidNotReceiveWithAnyArgs().EnsureResourceByIdentityAsync<Model.Patient>(null, null, null);
        }

        [Fact]
        public async void GivenMismatchedDeviceAndPatientIdReference_WhenResolveResourceIdentitiesAsync_ThenPatientDeviceMismatchExceptionThrown_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            var resourceService = Substitute.For<ResourceManagementService>(fhirClient);

            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Patient/abc"),
            };

            var patient = new Model.Patient
            {
                Id = "123",
            };

            resourceService.EnsureResourceByIdentityAsync<Model.Device>(null, null, null).ReturnsForAnyArgs(Task.FromResult(device));
            resourceService.EnsureResourceByIdentityAsync<Model.Patient>(null, null, null).ReturnsForAnyArgs(Task.FromResult(patient));

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");
            mg.PatientId.Returns("patientId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<Model.Device>(new FhirResourceNotFoundException(ResourceType.Patient)));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                await Assert.ThrowsAsync<PatientDeviceMismatchException>(() => idSrv.ResolveResourceIdentitiesAsync(mg));
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
            await resourceService.Received(1).EnsureResourceByIdentityAsync("deviceId", null, Arg.Any<Action<Model.Device, Model.Identifier>>());
            await resourceService.Received(1).EnsureResourceByIdentityAsync("patientId", null, Arg.Any<Action<Model.Patient, Model.Identifier>>());
        }

        [Fact]
        public async void GivenValidEncounterIdentifier_WhenResolveResourceIdentitiesAsync_ThenEncounterIdReturned_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            var resourceService = Substitute.For<ResourceManagementService>(fhirClient);
            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Patient/123"),
            };

            var encounter = new Model.Encounter
            {
                Id = "abc",
            };

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");
            mg.EncounterId.Returns("eId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(device));

            resourceService.GetResourceByIdentityAsync<Model.Encounter>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(encounter));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal("1", ids[ResourceType.Device]);
                Assert.Equal("123", ids[ResourceType.Patient]);
                Assert.Equal("abc", ids[ResourceType.Encounter]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Encounter>("eId", null);
        }

        [Fact]
        public async void GivenInValidEncounterIdentifier_WhenResolveResourceIdentitiesAsync_ThenFhirResourceNotFoundExceptionThrown_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            var resourceService = Substitute.For<ResourceManagementService>(fhirClient);
            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Patient/123"),
            };

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");
            mg.EncounterId.Returns("eId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(device));

            resourceService.GetResourceByIdentityAsync<Model.Encounter>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult((Model.Encounter)null));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                var ex = await Assert.ThrowsAsync<FhirResourceNotFoundException>(async () => await idSrv.ResolveResourceIdentitiesAsync(mg));
                Assert.Equal(ResourceType.Encounter, ex.FhirResourceType);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Encounter>("eId", null);
        }
    }
}
