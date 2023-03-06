// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Common.Config;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementGroupFhirImportServiceTests
    {
        [Fact]
        public async void GivenMeasurementGroupEvent_WhenProcessEventsAsync_ThenProcessAsyncInvokedAndCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new MeasurementGroupFhirImportService(fhirService, options, exceptionTelemetryProcessor);

            JArray o = JArray.Parse(
            @"[{
                'Type': 'summary',
                'OccurrenceTimeUtc': '2020-08-10T00:15:00Z',
                'IngestionTimeUtc': '2022-08-10T19:29:56.993Z',
                'DeviceId': 'ABC',
                'PatientId': '123',
                'EncounterId': null,
                'CorrelationId': null,
                'Properties': [
                    { 'Name': 'testdata1','Value':'1'},
                    { 'Name': 'testdata2','Value':'2'}
                ]
            }]");

            var jsonString = JsonConvert.SerializeObject(o, Formatting.None);
            var contentBytes = Encoding.UTF8.GetBytes(jsonString);

            // set IsMeasurementGroup property
            var events = new List<EventMessage>()
            {
                new EventMessage("0", contentBytes, null, 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>() { { "IsMeasurementGroup", true } }, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
            };

            await fhirImport.ProcessEventsAsync(events, string.Empty, log, default);

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(1).ProcessAsync(default, default, default);
        }

        [Fact]
        public async void GivenCompressedMeasurementGroupEvent_WhenProcessEventsAsync_ThenProcessAsyncInvokedAndCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new MeasurementGroupFhirImportService(fhirService, options, exceptionTelemetryProcessor);

            JArray o = JArray.Parse(
            @"[{
                'Type': 'summary',
                'OccurrenceTimeUtc': '2020-08-10T00:15:00Z',
                'IngestionTimeUtc': '2022-08-10T19:29:56.993Z',
                'DeviceId': 'ABC',
                'PatientId': '123',
                'EncounterId': null,
                'CorrelationId': null,
                'Properties': [
                    { 'Name': 'testdata1','Value':'1'},
                    { 'Name': 'testdata2','Value':'2'}
                ]
            }]");

            var jsonString = JsonConvert.SerializeObject(o, Formatting.None);
            var contentBytes = Encoding.UTF8.GetBytes(jsonString);

            // compress the bytes
            var compressedBytes = Common.IO.Compression.CompressWithGzip(contentBytes);

            // set IsMeasurementGroup property
            // set ContentType to application/gzip
            // set Body to be the compressed bytes
            var events = new List<EventMessage>()
            {
                new EventMessage("0", compressedBytes, Common.IO.Compression.GzipContentType, 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>() { { "IsMeasurementGroup", true } }, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
            };

            await fhirImport.ProcessEventsAsync(events, string.Empty, log, default);

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(1).ProcessAsync(default, default, default);
        }

        [Fact]
        public async void GivenMixOfMeasurementsAndMeasurementGroups_WhenProcessEventsAsync_ThenProcessAsyncInvokedAndCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new MeasurementGroupFhirImportService(fhirService, options, exceptionTelemetryProcessor);

            JObject evt1 = JObject.Parse(
            @$"{{
                'Type': 'summary',
                'OccurrenceTimeUtc': '2020-08-10T00:15:00Z',
                'IngestionTimeUtc': '2022-08-10T19:29:55.993Z',
                'DeviceId': 'ABC',
                'PatientId': '123',
                'EncounterId': null,
                'CorrelationId': null,
                'Properties': [
                    {{ 'Name': 'testdata1','Value':'1'}},
                    {{ 'Name': 'testdata2','Value':'2'}}
                ]
            }}");

            JArray evt2 = JArray.Parse(
            @"[{
                'Type': 'summary',
                'OccurrenceTimeUtc': '2020-08-10T00:15:00Z',
                'IngestionTimeUtc': '2022-08-10T19:29:56.993Z',
                'DeviceId': 'ABC',
                'PatientId': '123',
                'EncounterId': null,
                'CorrelationId': null,
                'Properties': [
                    { 'Name': 'testdata1','Value':'1'},
                    { 'Name': 'testdata2','Value':'2'}
                ]
            }]");

            var jsonString = JsonConvert.SerializeObject(evt2, Formatting.None);
            var contentBytes = Encoding.UTF8.GetBytes(jsonString);

            // compress the bytes
            var compressedBytes = Common.IO.Compression.CompressWithGzip(contentBytes);

            // set IsMeasurementGroup property
            // set ContentType to application/gzip
            // set Body to be the compressed bytes
            var events = new List<EventMessage>()
            {
                new EventMessage("0", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evt1, Formatting.None)), null, 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
                new EventMessage("0", compressedBytes, Common.IO.Compression.GzipContentType, 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>() { { "IsMeasurementGroup", true } }, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
            };

            await fhirImport.ProcessEventsAsync(events, string.Empty, log, default);

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(2).ProcessAsync(default, default, default);
        }

        [Fact]
        public async void GivenMultipleMeasurementEventsForSamePatientAndDevice_WhenProcessEventsAsync_ThenProcessAsyncInvokedWithExpectedMeasurementGroupData_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new MeasurementGroupFhirImportService(fhirService, options, exceptionTelemetryProcessor);

            // Mock event data for the same patient and device, with 2/3 events having the same 'OccurrenceTimeUtc' and with 'IngestionTimeUtc' 1s apart.
            var event1OccurrenceTimeUtc = new DateTime(2020, 08, 10, 00, 15, 00);
            var event1IngestionTimeUtc = new DateTime(2022, 08, 10, 12, 00, 00);
            JObject evt1 = JObject.Parse(
            @$"{{
                'Type': 'summary',
                'OccurrenceTimeUtc': '{event1OccurrenceTimeUtc:o}',
                'IngestionTimeUtc': '{event1IngestionTimeUtc:o}',
                'DeviceId': 'ABC',
                'PatientId': '123',
                'EncounterId': null,
                'CorrelationId': null,
                'Properties': [
                    {{ 'Name': 'testdata1','Value':'1'}},
                    {{ 'Name': 'testdata2','Value':'2'}}
                ]
            }}");

            var event2OccurrenceTimeUtc = event1OccurrenceTimeUtc;
            var event2IngestionTimeUtc = event1IngestionTimeUtc.AddSeconds(1);
            JObject evt2 = new (evt1)
            {
                ["IngestionTimeUtc"] = event2IngestionTimeUtc,
            };

            var event3OccurrenceTimeUtc = event1OccurrenceTimeUtc.AddMinutes(1);
            var event3IngestionTimeUtc = event2IngestionTimeUtc.AddSeconds(1);
            JObject evt3 = new (evt1)
            {
                ["OccurrenceTimeUtc"] = event3OccurrenceTimeUtc,
                ["IngestionTimeUtc"] = event3IngestionTimeUtc,
            };

            JArray eventGroup = new JArray
            {
                evt1,
                evt2,
                evt3,
            };

            var jsonString = JsonConvert.SerializeObject(eventGroup, Formatting.None);
            var contentBytes = Encoding.UTF8.GetBytes(jsonString);

            // compress the bytes
            var compressedBytes = Common.IO.Compression.CompressWithGzip(contentBytes);

            // set IsMeasurementGroup property
            // set ContentType to application/gzip
            // set Body to be the compressed bytes
            var events = new List<EventMessage>()
            {
                new EventMessage("0", compressedBytes, Common.IO.Compression.GzipContentType, 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>() { { "IsMeasurementGroup", true } }, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
            };

            await fhirImport.ProcessEventsAsync(events, string.Empty, log, default);

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.Received(1).ProcessAsync(
               Arg.Any<ILookupTemplate<IFhirTemplate>>(),
               Arg.Is<IMeasurementGroup>(m =>
               m.Data.Count() == 2 &&
               m.Data.ElementAt(0).IngestionTimeUtc == event2IngestionTimeUtc &&
               m.Data.ElementAt(0).OccurrenceTimeUtc == event2OccurrenceTimeUtc &&
               m.Data.ElementAt(1).IngestionTimeUtc == event3IngestionTimeUtc &&
               m.Data.ElementAt(1).OccurrenceTimeUtc == event3OccurrenceTimeUtc),
               default);
        }

        private MeasurementFhirImportOptions BuildMockOptions()
        {
            var exceptionProcessor = Substitute.For<ExceptionTelemetryProcessor>();
            var templateFactory = Substitute.For<ITemplateFactory<string, ITemplateContext<ILookupTemplate<IFhirTemplate>>>>();
            var parallelTaskOptions = new ParallelTaskOptions();

            var options = Substitute.For<MeasurementFhirImportOptions>();

            options.ParallelTaskOptions.Returns(parallelTaskOptions);
            options.TemplateFactory.Returns(templateFactory);

            return options;
        }

        private class TestFhirImportService : MeasurementGroupFhirImportService
        {
            public TestFhirImportService(FhirImportService fhirImportService, MeasurementFhirImportOptions options, IExceptionTelemetryProcessor exceptionTelemetryProcessor)
                : base(fhirImportService, options, exceptionTelemetryProcessor)
            {
            }

            public int WorkItemCount { get; private set; }

            protected override Task StartWorker(IEnumerable<Func<Task>> workItems)
            {
                var workItemArray = workItems.ToArray();
                WorkItemCount = workItemArray.Length;
                return base.StartWorker(workItemArray);
            }
        }
    }
}
