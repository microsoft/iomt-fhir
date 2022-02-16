// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Common;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Fhir.Client;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Model = Hl7.Fhir.Model;
using ResourceType = Microsoft.Health.Fhir.Ingest.Data.ResourceType;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4FhirImportServiceTests
    {
        [Fact]
        public async void GivenValidData_WhenProcessAsync_ThenSaveObservationInvokedForEachObservationGroup_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();

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

            var logger = Substitute.For<ITelemetryLogger>();

            var service = Substitute.ForPartsOf<R4FhirImportService>(identityService, fhirClient, templateProcessor, cache, logger)
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
            var fhirClient = Utilities.CreateMockFhirService();
            fhirClient.CreateResourceAsync(Arg.Any<Model.Observation>()).ReturnsForAnyArgs(Task.FromResult(new Model.Observation()));

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()));
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var logger = Substitute.For<ITelemetryLogger>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache, logger);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            logger.Received(1).LogMetric(Arg.Is<Metric>(x => Equals("ObservationCreated", x.Dimensions[DimensionNames.Name])), 1);
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

            var fhirClient = Utilities.CreateMockFhirService();
            fhirClient.CreateResourceAsync(Arg.Any<Model.Observation>()).ReturnsForAnyArgs(Task.FromResult(new Model.Observation()));
            fhirClient.SearchForResourceAsync(Arg.Any<Model.ResourceType>(), Arg.Any<string>()).ReturnsForAnyArgs(Task.FromResult(foundBundle));
            fhirClient.UpdateResourceAsync(Arg.Any<Model.Observation>()).ReturnsForAnyArgs(Task.FromResult(savedObservation));

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()))
                .Mock(m => m.MergeObservation(default, default, default).ReturnsForAnyArgs(new Model.Observation() { Id = "2" }));

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var logger = Substitute.For<ITelemetryLogger>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache, logger);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            templateProcessor.ReceivedWithAnyArgs(1).MergeObservation(default, default, default);
            logger.Received(1).LogMetric(Arg.Is<Metric>(x => Equals("ObservationUpdated", x.Dimensions[DimensionNames.Name])), 1);
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

            var fhirClient = Utilities.CreateMockFhirService();
            fhirClient.SearchForResourceAsync(Arg.Any<Model.ResourceType>(), Arg.Any<string>()).ReturnsForAnyArgs(Task.FromResult(foundBundle1));
            fhirClient.UpdateResourceAsync(Arg.Any<Model.Observation>())
                .Returns(
                    x => { throw new FhirException(new FhirResponse<OperationOutcome>(new HttpResponseMessage(HttpStatusCode.Conflict), new OperationOutcome())); },
                    x => Task.FromResult(savedObservation));

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()))
                .Mock(m => m.MergeObservation(default, default, default).ReturnsForAnyArgs(foundObservation2));

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var logger = Substitute.For<ITelemetryLogger>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache, logger);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            templateProcessor.ReceivedWithAnyArgs(2).MergeObservation(default, default, default);
            logger.Received(1).LogMetric(Arg.Is<Metric>(x => Equals("ObservationUpdated", x.Dimensions[DimensionNames.Name])), 1);
        }

        [Fact]
        public void GivenValidTemplate_WhenGenerateObservation_ExpectedReferencesSet_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();

            var ids = BuildIdCollection();
            ids[ResourceType.Encounter] = "encounterId";

            var identityService = Substitute.For<IResourceIdentityService>();
            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()));
            var cache = Substitute.For<IMemoryCache>();

            var observationGroup = Substitute.For<IObservationGroup>();
            var identifer = new Model.Identifier();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var logger = Substitute.For<ITelemetryLogger>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache, logger);

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

        [Fact]
        public async Task GivenCachedObservationDeleted_WhenGenerateObservation_ThenCacheIsUpdatedAndCreateInvoked_Test()
        {
            var savedObservation = new Model.Observation { Id = "1" };

            // Mock the cached Observation
            var cache = Substitute.For<IMemoryCache>();
            cache.TryGetValue(Arg.Any<object>(), out Model.Observation observation)
                .Returns(x =>
                {
                    x[1] = new Model.Observation();
                    return true;
                });

            var fhirClient = Utilities.CreateMockFhirService();
            fhirClient.UpdateResourceAsync(Arg.Any<Model.Observation>()).ThrowsForAnyArgs(new FhirException(new FhirResponse<OperationOutcome>(new HttpResponseMessage(HttpStatusCode.Conflict), new OperationOutcome())));
            fhirClient.SearchForResourceAsync(Arg.Any<Model.ResourceType>(), Arg.Any<string>()).ReturnsForAnyArgs(Task.FromResult(new Model.Bundle()));
            fhirClient.CreateResourceAsync(Arg.Any<Model.Observation>()).ReturnsForAnyArgs(Task.FromResult(savedObservation));

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.CreateObservation(default, default).ReturnsForAnyArgs(new Model.Observation()))
                .Mock(m => m.MergeObservation(default, default, default).ReturnsForAnyArgs(new Model.Observation { Id = "2" }));

            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var logger = Substitute.For<ITelemetryLogger>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache, logger);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            var test = IomtMetrics.FhirResourceSaved(ResourceType.Observation, ResourceOperation.Created);

            templateProcessor.ReceivedWithAnyArgs(1).MergeObservation(default, default, default);
            cache.Received(1).Remove(Arg.Any<string>());
            logger.Received(1).LogMetric(Arg.Is<Metric>(x => Equals("ObservationCreated", x.Dimensions[DimensionNames.Name])), 1);
            cache.Received(1).Set(Arg.Any<object>(), savedObservation);
        }

        [Fact]
        public async Task GivenCachedObservationUnchanged_WhenGenerateObservation_ThenCacheNoOperation_Test()
        {
            var cachedObservation = new Model.Observation { Id = "1" };

            // Mock the cached Observation
            var cache = Substitute.For<IMemoryCache>();
            cache.TryGetValue(Arg.Any<object>(), out Model.Observation observation)
                .Returns(x =>
                {
                    x[1] = cachedObservation;
                    return true;
                });

            var fhirClient = Utilities.CreateMockFhirService();

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.MergeObservation(default, default, default).ReturnsForAnyArgs(cachedObservation));

            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var logger = Substitute.For<ITelemetryLogger>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache, logger);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            var test = IomtMetrics.FhirResourceSaved(ResourceType.Observation, ResourceOperation.Created);

            templateProcessor.ReceivedWithAnyArgs(1).MergeObservation(default, default, default);
            logger.Received(1).LogMetric(Arg.Is<Metric>(x => Equals("ObservationNoOperation", x.Dimensions[DimensionNames.Name])), 1);
            cache.Received(1).Set(Arg.Any<object>(), cachedObservation);
        }

        [Fact]
        public async void GivenFoundObservationUnchanged_WhenSaveObservationAsync_ThenUpdateInvoked_Test()
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

            var fhirClient = Utilities.CreateMockFhirService();
            fhirClient.SearchForResourceAsync(Arg.Any<Model.ResourceType>(), Arg.Any<string>()).ReturnsForAnyArgs(Task.FromResult(foundBundle));
            fhirClient.UpdateResourceAsync(Arg.Any<Model.Observation>()).ReturnsForAnyArgs(Task.FromResult(savedObservation));

            var ids = BuildIdCollection();
            var identityService = Substitute.For<IResourceIdentityService>()
                .Mock(m => m.ResolveResourceIdentitiesAsync(default).ReturnsForAnyArgs(Task.FromResult(ids)));

            var observationGroup = Substitute.For<IObservationGroup>();

            var templateProcessor = Substitute.For<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Model.Observation>>()
                .Mock(m => m.MergeObservation(default, default, default).ReturnsForAnyArgs(foundObservation));

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ILookupTemplate<IFhirTemplate>>();

            var logger = Substitute.For<ITelemetryLogger>();

            var service = new R4FhirImportService(identityService, fhirClient, templateProcessor, cache, logger);

            var result = await service.SaveObservationAsync(config, observationGroup, ids);

            templateProcessor.ReceivedWithAnyArgs(1).MergeObservation(default, default, default);
            logger.Received(1).LogMetric(Arg.Is<Metric>(x => Equals("ObservationNoOperation", x.Dimensions[DimensionNames.Name])), 1);
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
            throw new FhirException(new FhirResponse<OperationOutcome>(new HttpResponseMessage(HttpStatusCode.Conflict), new OperationOutcome()));
        }
    }
}
