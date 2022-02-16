// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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
    public class R4DeviceAndPatientLookupIdentityServiceTests
    {
        [Fact]
        public async void GivenValidDeviceIdentifier_WhenResolveResourceIdentitiesAsync_ThenDeviceAndPatientIdReturned_Test()
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

            using (var idSrv = new R4DeviceAndPatientLookupIdentityService(fhirClient, resourceService))
            {
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal("1", ids[ResourceType.Device]);
                Assert.Equal("123", ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
        }

        [Fact]
        public async void GivenValidDeviceIdentifierWhenDefaultSystemSet_WhenResolveResourceIdentitiesAsync_ThenDeviceAndPatientIdReturned_Test()
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

            var options = new ResourceIdentityOptions
            {
                DefaultDeviceIdentifierSystem = "mySystem",
            };

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(device));

            using (var idSrv = new R4DeviceAndPatientLookupIdentityService(fhirClient, resourceService))
            {
                idSrv.Initialize(options);
                var ids = await idSrv.ResolveResourceIdentitiesAsync(mg);

                Assert.Equal("1", ids[ResourceType.Device]);
                Assert.Equal("123", ids[ResourceType.Patient]);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", "mySystem");
        }

        [Fact]
        public async void GivenDeviceWithNotPatientReference_WhenResolveResourceIdentitiesAsync_ThenFhirResourceNotFoundExceptionThrown_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            var resourceService = Substitute.For<ResourceManagementService>(fhirClient);
            Model.Device device = new Model.Device();

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(device));

            using (var idSrv = new R4DeviceAndPatientLookupIdentityService(fhirClient, resourceService))
            {
                var ex = await Assert.ThrowsAsync<FhirResourceNotFoundException>(async () => await idSrv.ResolveResourceIdentitiesAsync(mg));
                Assert.Equal(ResourceType.Patient, ex.FhirResourceType);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
        }

        [Fact]
        public async void GivenDeviceWithPatientReferenceUnsupportedCharacters_WhenResolveResourceIdentitiesAsync_ThenNotSupportedExceptionThrown_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            var resourceService = Substitute.For<ResourceManagementService>(fhirClient);
            var device = new Model.Device
            {
                Id = "1",
                Patient = new Model.ResourceReference("Not a reference in the form of: /ResourceName/Identifier"),
            };

            var mg = Substitute.For<IMeasurementGroup>();
            mg.DeviceId.Returns("deviceId");

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(device));

            using (var idSrv = new R4DeviceAndPatientLookupIdentityService(fhirClient, resourceService))
            {
                var ex = await Assert.ThrowsAsync<FhirResourceNotFoundException>(async () => await idSrv.ResolveResourceIdentitiesAsync(mg));
                Assert.Equal(ResourceType.Patient, ex.FhirResourceType);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
        }
    }
}
