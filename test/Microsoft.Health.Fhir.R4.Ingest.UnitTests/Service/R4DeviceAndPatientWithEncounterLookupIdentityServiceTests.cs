// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Health.Common;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Ingest.Data;
using NSubstitute;
using Xunit;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4DeviceAndPatientWithEncounterLookupIdentityServiceTests
    {
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

            using (var idSrv = new R4DeviceAndPatientWithEncounterLookupIdentityService(fhirClient, resourceService))
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

            using (var idSrv = new R4DeviceAndPatientWithEncounterLookupIdentityService(fhirClient, resourceService))
            {
                var ex = await Assert.ThrowsAsync<FhirResourceNotFoundException>(async () => await idSrv.ResolveResourceIdentitiesAsync(mg));
                Assert.Equal(ResourceType.Encounter, ex.FhirResourceType);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Encounter>("eId", null);
        }

        [Fact]
        public async void GivenNoEncounterIdentifier_WhenResolveResourceIdentitiesAsync_ThenResourceIdentityNotDefinedExceptionThrown_Test()
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
            mg.EncounterId.Returns((string)null);

            resourceService.GetResourceByIdentityAsync<Model.Device>(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(device));

            using (var idSrv = new R4DeviceAndPatientWithEncounterLookupIdentityService(fhirClient, resourceService))
            {
                var ex = await Assert.ThrowsAsync<ResourceIdentityNotDefinedException>(async () => await idSrv.ResolveResourceIdentitiesAsync(mg));
                Assert.Equal(ResourceType.Encounter, ex.FhirResourceType);
            }

            await resourceService.Received(1).GetResourceByIdentityAsync<Model.Device>("deviceId", null);
            await resourceService.DidNotReceiveWithAnyArgs().GetResourceByIdentityAsync<Model.Encounter>(null, null);
        }
    }
}
