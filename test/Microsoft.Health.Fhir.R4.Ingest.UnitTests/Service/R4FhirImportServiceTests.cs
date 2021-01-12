﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
            var fhirClient = Utilities.CreateMockFhirClient();

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
            // Mock search and update request
            var handler = Utilities.CreateMockMessageHandler()
                .Mock(m => m.GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Get)).Returns(new Model.Bundle()))
                .Mock(m => m.GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post)).Returns(new Model.Observation()));

            var fhirClient = Utilities.CreateMockFhirClient(handler);

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

            handler.Received(1).GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Get));
            handler.Received(1).GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post));
        }

        [Fact]
        public async void GivenFoundObservation_WhenSaveObservationAsync_ThenUpdateInvoked_Test()
        {
            var foundObservation = new Model.Observation { Id = "1" };
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

            // Mock search and update request
            var handler = Utilities.CreateMockMessageHandler()
                .Mock(m => m.GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Get)).Returns(foundBundle))
                .Mock(m => m.GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Put)).Returns(savedObservation));

            var fhirClient = Utilities.CreateMockFhirClient(handler);

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()))
                .Mock(m => m.MergeObservation(default, default, default).ReturnsForAnyArgs(foundObservation));

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            templateProcessor.ReceivedWithAnyArgs(1).MergeObservation(default, default, default);
            handler.Received(1).GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Get));
            handler.Received(1).GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Put));
        }

        [Fact]
        public async void GivenFoundObservationAndConflictOnSave_WhenSaveObservationAsync_ThenGetAndUpdateInvoked_Test()
        {
            var foundObservation1 = new Model.Observation { Id = "1" };
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

            var foundObservation2 = new Model.Observation { Id = "2" };
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

            // Mock search and update request
            var handler = Utilities.CreateMockMessageHandler()
                .Mock(m => m.GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Get)).Returns(foundBundle1, foundBundle2))
                .Mock(m => m.GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Put)).Returns(x => ThrowConflictException(), x => savedObservation));

            var fhirClient = Utilities.CreateMockFhirClient(handler);

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()))
                .Mock(m => m.MergeObservation(default, default, default).ReturnsForAnyArgs(foundObservation2));

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            templateProcessor.ReceivedWithAnyArgs(2).MergeObservation(default, default, default);
            handler.Received(2).GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Get));
            handler.Received(2).GetReturnContent(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Put));
        }

        [Fact]
        public void GivenValidTemplate_WhenGenerateObservation_ExpectedReferencesSet_Test()
        {
            var fhirClient = Utilities.CreateMockFhirClient();

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
