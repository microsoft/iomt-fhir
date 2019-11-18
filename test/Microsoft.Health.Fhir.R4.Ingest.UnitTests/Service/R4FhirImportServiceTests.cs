// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4FhirImportServiceTests
    {
        [Fact]
        public async void GivenValidData_WhenProcessAsync_ThenSaveObservationInvokedForEachObservationGroup_Test()
        {
            var fhirClient = Substitute.For<IFhirClient>();

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroups = new IObservationGroup[]
            {
                Substitute.For<IObservationGroup>(),
                Substitute.For<IObservationGroup>(),
            };

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservationGroups(default, default).ReturnsForAnyArgs(observationGroups))
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()));
            var cache = Substitute.For<IMemoryCache>();

            var measurementGroup = Substitute.For<IMeasurementGroup>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var service = Substitute.ForPartsOf<R4FhirImportService>(identityService, fhirClient, templateProcessor, cache)
                .Mock(m => m.SaveObservationAsync(default, default, default).ReturnsForAnyArgs(string.Empty));

            await service.ProcessAsync(config, measurementGroup);

            await identityService.Received(1).ResolveResourceIdentitiesAsync(measurementGroup);
            templateProcessor.Received(1).CreateObservationGroups(config, measurementGroup);

            await service.Received(1).SaveObservationAsync(config, observationGroups[0], ids);
            await service.Received(1).SaveObservationAsync(config, observationGroups[1], ids);
        }

        [Fact]
        public async void GivenNotFoundObservation_WhenSaveObservationAsync_ThenCreateInvoked_Test()
        {
            var fhirClient = Substitute.For<IFhirClient>()
                .Mock(m => m.CreateAsync<Model.Observation>(default).ReturnsForAnyArgs(new Model.Observation()));

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()));
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            await fhirClient.ReceivedWithAnyArgs(1).CreateAsync<Model.Observation>(default);
            await fhirClient.ReceivedWithAnyArgs(1).SearchAsync<Model.Observation>(default);
        }

        [Fact]
        public async void GivenFoundObservation_WhenSaveObservationAsync_ThenUpdateInvoked_Test()
        {
            var foundObservation = new Model.Observation();
            var foundBundle = new Model.Bundle
            {
                Entry = new List<Model.Bundle.EntryComponent>
                {
                    new Model.Bundle.EntryComponent
                    {
                        Resource = foundObservation,
                    },
                },
            };

            var savedObservation = new Model.Observation();

            var fhirClient = Substitute.For<IFhirClient>()
                .Mock(m => m.UpdateAsync<Model.Observation>(default, default).ReturnsForAnyArgs(savedObservation))
                .Mock(m => m.SearchAsync<Model.Observation>(default).ReturnsForAnyArgs(foundBundle));

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()));
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            templateProcessor.ReceivedWithAnyArgs(1).MergeObservation(default, default, default);
            await fhirClient.ReceivedWithAnyArgs(1).UpdateAsync<Model.Observation>(default);
            await fhirClient.ReceivedWithAnyArgs(1).SearchAsync<Model.Observation>(default);
        }

        [Fact]
        public async void GivenFoundObservationAndConflictOnSave_WhenSaveObservationAsync_ThenGetAndUpdateInvoked_Test()
        {
            var foundObservation1 = new Model.Observation();
            var foundBundle1 = new Model.Bundle
            {
                Entry = new List<Model.Bundle.EntryComponent>
                {
                    new Model.Bundle.EntryComponent
                    {
                        Resource = foundObservation1,
                    },
                },
            };

            var foundObservation2 = new Model.Observation();
            var foundBundle2 = new Model.Bundle
            {
                Entry = new List<Model.Bundle.EntryComponent>
                {
                    new Model.Bundle.EntryComponent
                    {
                        Resource = foundObservation2,
                    },
                },
            };

            var savedObservation = new Model.Observation();

            var fhirClient = Substitute.For<IFhirClient>()
                .Mock(m => m.UpdateAsync<Model.Observation>(default, default)
                    .ReturnsForAnyArgs(x => ThrowConflictException(), x => savedObservation))
                .Mock(m => m.SearchAsync<Model.Observation>(default).ReturnsForAnyArgs(foundBundle1, foundBundle2));

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()));
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            templateProcessor.ReceivedWithAnyArgs(2).MergeObservation(default, default, default);
            await fhirClient.ReceivedWithAnyArgs(2).UpdateAsync<Model.Observation>(default);
            await fhirClient.ReceivedWithAnyArgs(2).SearchAsync<Model.Observation>(default);
        }

        [Fact]
        public void GivenValidTemplate_WhenGenerateObservation_ExpectedReferencesSet_Test()
        {
            var fhirClient = Substitute.For<IFhirClient>();

            var ids = BuildIdCollection();
            ids[ResourceType.Encounter] = "encounterId";

            var identityService = Substitute.For<IResourceIdentityService>();
            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()));
            var cache = Substitute.For<IMemoryCache>();

            var observationGroup = Substitute.For<IObservationGroup>();
            var identifer = new Model.Identifier();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache);

            var result = service.GenerateObservation(config, observationGroup, identifer, ids);

            templateProcessor.Received(1).CreateObservation(config, observationGroup);

            Assert.Equal("Patient/patientId", result.Subject.Reference);
            Assert.Equal("Encounter/encounterId", result.Encounter.Reference);
            Assert.Equal("Device/deviceId", result.Device.Reference);
            Assert.Collection(
                result.Identifier,
                id =>
                {
                    Assert.Equal(identifer, id);
                });
        }

        private static IDictionary<Data.ResourceType, string> BuildIdCollection()
        {
            var lookup = IdentityLookupFactory.Instance.Create();
            lookup[ResourceType.Device] = "deviceId";
            lookup[ResourceType.Patient] = "patientId";
            return lookup;
        }

        private static Model.Observation ThrowConflictException()
        {
            throw new FhirOperationException("error", HttpStatusCode.Conflict);
        }
    }
}
