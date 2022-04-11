// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Health.Common;
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

            resourceService.EnsureResourceByIdentityAsync<Model.Device>(null, null, null).ReturnsForAnyArgs(Task.FromResult(device));
            resourceService.EnsureResourceByIdentityAsync<Model.Patient>(null, null, null).ReturnsForAnyArgs(Task.FromResult(patient));

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");
            mg.PatientId.Returns("patientId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<Model.Device>(new FhirResourceNotFoundException(ResourceType.Patient)));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal("1", ids[ResourceType.Device]);
                Assert.Equal("123", ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
            await resourceService.Received(1).EnsureResourceByIdentityAsync("deviceId", null, Arg.Any<Action<Model.Device, Model.Identifier>>());
            await resourceService.Received(1).EnsureResourceByIdentityAsync("patientId", null, Arg.Any<Action<Model.Patient, Model.Identifier>>());
        }

        [Fact]
        public async void GivenDeviceNotFoundException_WhenResolveResourceIdentitiesAsync_ThenDeviceAndPatientCreateInvokedAndIdsReturned_Test()
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

            resourceService.EnsureResourceByIdentityAsync<Model.Device>(null, null, null).ReturnsForAnyArgs(Task.FromResult(device));
            resourceService.EnsureResourceByIdentityAsync<Model.Patient>(null, null, null).ReturnsForAnyArgs(Task.FromResult(patient));

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");
            mg.PatientId.Returns("patientId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<Model.Device>(new FhirResourceNotFoundException(ResourceType.Device)));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal("1", ids[ResourceType.Device]);
                Assert.Equal("123", ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
            await resourceService.Received(1).EnsureResourceByIdentityAsync("deviceId", null, Arg.Any<Action<Model.Device, Model.Identifier>>());
            await resourceService.Received(1).EnsureResourceByIdentityAsync("patientId", null, Arg.Any<Action<Model.Patient, Model.Identifier>>());
        }

        [Fact]
        public async void GivenDeviceNotFoundExceptionWithDeviceSystemSet_WhenResolveResourceIdentitiesAsync_ThenDeviceAndPatientCreateInvokedWithDeviceSystemAndIdsReturned_Test()
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

            resourceService.EnsureResourceByIdentityAsync<Model.Device>(null, null, null).ReturnsForAnyArgs(Task.FromResult(device));
            resourceService.EnsureResourceByIdentityAsync<Model.Patient>(null, null, null).ReturnsForAnyArgs(Task.FromResult(patient));

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");
            mg.PatientId.Returns("patientId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<Model.Device>(new FhirResourceNotFoundException(ResourceType.Device)));

            using (var idSrv = new R4DeviceAndPatientCreateIdentityService(fhirClient, resourceService))
            {
                idSrv.Initialize(new ResourceIdentityOptions { DefaultDeviceIdentifierSystem = "mySystem" });
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal("1", ids[ResourceType.Device]);
                Assert.Equal("123", ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", "mySystem");
            await resourceService.Received(1).EnsureResourceByIdentityAsync("deviceId", "mySystem", Arg.Any<Action<Model.Device, Model.Identifier>>());
            await resourceService.Received(1).EnsureResourceByIdentityAsync("patientId", null, Arg.Any<Action<Model.Patient, Model.Identifier>>());
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
    }
}
