// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Health.Common.Config;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Exceptions;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementFhirImportServiceTests
    {
        [Fact]
        public async void GivenEmptyStream_WhenParseStreamAsync_ThenCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new MeasurementFhirImportService(fhirService, options, exceptionTelemetryProcessor);

            var stream = Substitute.For<Stream>();
            stream.CanRead.Returns(true);

            using (stream)
            {
                await fhirImport.ProcessStreamAsync(stream, string.Empty, log);
            }

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.DidNotReceiveWithAnyArgs().ProcessAsync(default, default, default);
        }

        [Fact]
        public async void GivenMeasurementStream_WhenParseStreamAsync_ThenProcessAsyncInvokedPerGroupAndCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new MeasurementFhirImportService(fhirService, options, exceptionTelemetryProcessor);

            var measurements = new IMeasurementGroup[] { Substitute.For<IMeasurementGroup>(), Substitute.For<IMeasurementGroup>() };

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty, log);

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(2).ProcessAsync(default, null, default);
        }

        [Fact]
        public async void GivenExceptionDuringParseStreamAsync_WhenParseStreamAsync_ThenProcessAsyncThrowsException_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();

            var exception = new InvalidOperationException();
            var fhirService = Substitute.For<FhirImportService>();
            fhirService.ProcessAsync(default, default, default).ReturnsForAnyArgs(Task.FromException(exception));

            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();
            var fhirImport = new MeasurementFhirImportService(fhirService, options, exceptionTelemetryProcessor);

            var measurements = new MeasurementGroup[] { Substitute.For<MeasurementGroup>(), Substitute.For<MeasurementGroup>() };

            var aggEx = await Assert.ThrowsAsync<AggregateException>(async () => await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty, log));

            Assert.Collection(
                aggEx.InnerExceptions,
                ex =>
                {
                    Assert.Equal(exception, ex);
                });

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(1).ProcessAsync(default, default, default);
        }

        [Fact]
        public async void GivenExceptionDuringParseStreamAsync_WhenParseStreamAsyncAndExceptionProcessorHandles_ThenCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();

            var exceptionProcessor = Substitute.For<IExceptionTelemetryProcessor>();
            exceptionProcessor.HandleException(null, null).ReturnsForAnyArgs(true);

            var exception = new InvalidOperationException();

            var fhirService = Substitute.For<FhirImportService>();
            fhirService.ProcessAsync(default, default, default).ReturnsForAnyArgs(Task.FromException(exception));

            var fhirImport = new MeasurementFhirImportService(fhirService, options, exceptionProcessor);

            var measurements = new IMeasurementGroup[] { Substitute.For<IMeasurementGroup>(), Substitute.For<IMeasurementGroup>() };

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty, log);

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(2).ProcessAsync(default, default, default);
            exceptionProcessor.Received(2).HandleException(exception, log);
        }

        [Fact]
        public async void GivenSameDeviceIdInMeasurementGroup_WhenParseStreamAsync_ThenStartWorkerCountOne_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new TestFhirImportService(fhirService, options, exceptionProcessor);

            var measurements = new IMeasurementGroup[]
            {
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("1")),
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("1")),
            };

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty, log);

            Assert.Equal(1, fhirImport.WorkItemCount);
            await fhirService.ReceivedWithAnyArgs(2).ProcessAsync(default, default, default);
        }

        [Fact]
        public async void GivenMultipleDeviceIdInMeasurementGroup_WhenParseStreamAsync_ThenStartWorkerCountPerId_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();

            var exceptionProcessor = Substitute.For<IExceptionTelemetryProcessor>();
            var fhirImport = new TestFhirImportService(fhirService, options, exceptionProcessor);

            var measurements = new IMeasurementGroup[]
            {
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("1")),
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("1")),
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("2")),
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("3")),
            };

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty, log);

            Assert.Equal(3, fhirImport.WorkItemCount);
            await fhirService.ReceivedWithAnyArgs(4).ProcessAsync(default, default, default);
        }

        [Fact]
        public async void GivenMultipleMeasurementsInMeasurementGroup_WhenParseStreamAsync_CorrectTelemetryLogged_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new TestFhirImportService(fhirService, options, exceptionProcessor);

            var measurements = new IMeasurementGroup[]
            {
                Substitute.For<IMeasurementGroup>()
                .Mock(m => m.DeviceId.Returns("1"))
                .Mock(m => m.Data.Returns(
                    new IMeasurement[]
                    {
                        new Measurement { IngestionTimeUtc = DateTime.UtcNow },
                        new Measurement { IngestionTimeUtc = DateTime.UtcNow },
                    })),
            };

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty, log);

            Assert.Equal(1, fhirImport.WorkItemCount);
            await fhirService.ReceivedWithAnyArgs(1).ProcessAsync(default, default, default);

            // Telemetry logging is async/non-blocking. Ensure enough time has pass so section is hit.
            await Task.Delay(1000);
            log.ReceivedWithAnyArgs(6).LogMetric(null, 0d);
        }

        [Fact]
        public async void GivenMeasurementEvent_WhenProcessEventsAsync_ThenProcessAsyncInvokedAndCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new MeasurementFhirImportService(fhirService, options, exceptionTelemetryProcessor);

            JObject o = JObject.Parse(
            @"{
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
            }");

            var jsonString = JsonConvert.SerializeObject(o, Formatting.None);
            var contentBytes = Encoding.UTF8.GetBytes(jsonString);

            var events = new List<EventMessage>()
            {
                new EventMessage("0", contentBytes, null, 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
            };

            await fhirImport.ProcessEventsAsync(events, string.Empty, log, default);

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(1).ProcessAsync(default, default, default);
        }

        [Fact]
        public async void GivenMultipleMeasurementEventsForSamePatientAndDevice_WhenProcessEventsAsync_ThenProcessAsyncInvokedWithExpectedMeasurementGroupData_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new MeasurementFhirImportService(fhirService, options, exceptionTelemetryProcessor);

            // Mock event data for the same patient and device, with 2/3 events having the same 'OccurrenceTimeUtc' and with 'IngestionTimeUtc' 1s apart.
            var event1OccurrenceTimeUtc = new DateTime(2020, 08, 10, 00, 15, 00);
            var event1IngestionTimeUtc = new DateTime(2022, 08, 10, 12, 00, 00);
            JObject event1 = JObject.Parse(
            @$"{{
                'Type': 'summary',
                'OccurrenceTimeUtc': '{event1OccurrenceTimeUtc}',
                'IngestionTimeUtc': '{event1IngestionTimeUtc}',
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
            JObject event2 = new (event1)
            {
                ["IngestionTimeUtc"] = event2IngestionTimeUtc,
            };

            var event3OccurrenceTimeUtc = event1OccurrenceTimeUtc.AddMinutes(1);
            var event3IngestionTimeUtc = event2IngestionTimeUtc.AddSeconds(1);
            JObject event3 = new (event1)
            {
                ["OccurrenceTimeUtc"] = event3OccurrenceTimeUtc,
                ["IngestionTimeUtc"] = event3IngestionTimeUtc,
            };

            var events = new List<EventMessage>()
            {
                new EventMessage("0",  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(event1, Formatting.None)), null, 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
                new EventMessage("0",  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(event2, Formatting.None)), null, 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
                new EventMessage("0",  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(event3, Formatting.None)), null, 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
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

        [Fact]
        public async void GivenCompressedMeasurementGroupEvent_WhenProcessEventsAsync_ThenCompressionNotSupportedExceptionThrown_Test()
        {
            var log = Substitute.For<ITelemetryLogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();
            var exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();

            var fhirImport = new MeasurementFhirImportService(fhirService, options, exceptionTelemetryProcessor);

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

            var ex = await Assert.ThrowsAsync<CompressionNotSupportedException>(async () => await fhirImport.ProcessEventsAsync(events, string.Empty, log, default));

            options.TemplateFactory.Received(1).Create(string.Empty);
            exceptionTelemetryProcessor.Received(1).HandleException(ex, log);
        }

        private static Stream ToStream(object obj)
        {
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            var serializer = new JsonSerializer();
            serializer.Serialize(streamWriter, obj);
            streamWriter.Flush();
            stream.Position = 0;

            return stream;
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

        private class TestFhirImportService : MeasurementFhirImportService
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
