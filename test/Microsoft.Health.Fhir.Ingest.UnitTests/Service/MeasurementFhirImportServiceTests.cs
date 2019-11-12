// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Common.Config;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementFhirImportServiceTests
    {
        [Fact]
        public async void GivenEmptyStream_WhenParseStreamAsync_ThenCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ILogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();

            var fhirImport = new MeasurementFhirImportService(fhirService, options, log);

            var stream = Substitute.For<Stream>();
            stream.CanRead.Returns(true);

            using (stream)
            {
                await fhirImport.ProcessStreamAsync(stream, string.Empty);
            }

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.DidNotReceiveWithAnyArgs().ProcessAsync(null, null);
        }

        [Fact]
        public async void GivenMeasurementStream_WhenParseStreamAsync_ThenProcessAsyncInvokedPerGroupAndCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ILogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();

            var fhirImport = new MeasurementFhirImportService(fhirService, options, log);

            var measurements = new IMeasurementGroup[] { Substitute.For<IMeasurementGroup>(), Substitute.For<IMeasurementGroup>() };

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty);

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(2).ProcessAsync(null, null);
        }

        [Fact]
        public async void GivenExceptionDuringParseStreamAsync_WhenParseStreamAsync_ThenProcessAsyncThrowsException_Test()
        {
            var log = Substitute.For<ILogger>();
            var options = BuildMockOptions();

            var exception = new InvalidOperationException();
            var fhirService = Substitute.For<FhirImportService>();
            fhirService.ProcessAsync(null, null).ReturnsForAnyArgs(Task.FromException(exception));

            var fhirImport = new MeasurementFhirImportService(fhirService, options, log);

            var measurements = new MeasurementGroup[] { Substitute.For<MeasurementGroup>(), Substitute.For<MeasurementGroup>() };

            var aggEx = await Assert.ThrowsAsync<AggregateException>(async () => await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty));

            Assert.Collection(
                aggEx.InnerExceptions,
                ex =>
                {
                    Assert.Equal(exception, ex);
                });

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(1).ProcessAsync(null, null);
        }

        [Fact]
        public async void GivenExceptionDuringParseStreamAsync_WhenParseStreamAsyncAndExceptionProcessorHandles_ThenCompleteSuccessfully_Test()
        {
            var log = Substitute.For<ILogger>();
            var options = BuildMockOptions();
            options.ExceptionService.HandleException(null, null).ReturnsForAnyArgs(true);

            var exception = new InvalidOperationException();
            var fhirService = Substitute.For<FhirImportService>();
            fhirService.ProcessAsync(null, null).ReturnsForAnyArgs(Task.FromException(exception));

            var fhirImport = new MeasurementFhirImportService(fhirService, options, log);

            var measurements = new IMeasurementGroup[] { Substitute.For<IMeasurementGroup>(), Substitute.For<IMeasurementGroup>() };

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty);

            options.TemplateFactory.Received(1).Create(string.Empty);
            await fhirService.ReceivedWithAnyArgs(2).ProcessAsync(null, null);
            options.ExceptionService.Received(2).HandleException(exception, log);
        }

        [Fact]
        public async void GivenSameDeviceIdInMeasurementGroup_WhenParseStreamAsync_ThenStartWorkerCountOne_Test()
        {
            var log = Substitute.For<ILogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();

            var fhirImport = new TestFhirImportService(fhirService, options, log);

            var measurements = new IMeasurementGroup[]
            {
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("1")),
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("1")),
            };

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty);

            Assert.Equal(1, fhirImport.WorkItemCount);
            await fhirService.ReceivedWithAnyArgs(2).ProcessAsync(null, null);
        }

        [Fact]
        public async void GivenMultipleDeviceIdInMeasurementGroup_WhenParseStreamAsync_ThenStartWorkerCountPerId_Test()
        {
            var log = Substitute.For<ILogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();

            var fhirImport = new TestFhirImportService(fhirService, options, log);

            var measurements = new IMeasurementGroup[]
            {
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("1")),
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("1")),
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("2")),
                Substitute.For<IMeasurementGroup>().Mock(m => m.DeviceId.Returns("3")),
            };

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty);

            Assert.Equal(3, fhirImport.WorkItemCount);
            await fhirService.ReceivedWithAnyArgs(4).ProcessAsync(null, null);
        }

        [Fact]
        public async void GivenMultipleMeasurementsInMeasurementGroup_WhenParseStreamAsync_CorrectTelemetryLogged_Test()
        {
            var log = Substitute.For<ILogger>();
            var options = BuildMockOptions();
            var fhirService = Substitute.For<FhirImportService>();

            var fhirImport = new TestFhirImportService(fhirService, options, log);

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

            await fhirImport.ProcessStreamAsync(ToStream(measurements), string.Empty);

            Assert.Equal(1, fhirImport.WorkItemCount);
            await fhirService.ReceivedWithAnyArgs(1).ProcessAsync(null, null);

            // Telemetry logging is async/non-blocking. Ensure enough time has pass so section is hit.
            await Task.Delay(1000);
            log.ReceivedWithAnyArgs(4).LogMetric(null, 0d, null);
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
            var templateFactory = Substitute.For<ITemplateFactory<string, ILookupTemplate<IFhirTemplate>>>();
            var parallelTaskOptions = new ParallelTaskOptions();

            var options = Substitute.For<MeasurementFhirImportOptions>();

            options.ParallelTaskOptions.Returns(parallelTaskOptions);
            options.ExceptionService.Returns(exceptionProcessor);
            options.TemplateFactory.Returns(templateFactory);

            return options;
        }

        private class TestFhirImportService : MeasurementFhirImportService
        {
            public TestFhirImportService(FhirImportService fhirImportService, MeasurementFhirImportOptions options, ILogger log)
                : base(fhirImportService, options, log)
            {
            }

            public int WorkItemCount { get; private set; } = 0;

            protected override Task StartWorker(IEnumerable<Func<Task>> workItems)
            {
                var workItemArray = workItems.ToArray();
                WorkItemCount = workItemArray.Length;
                return base.StartWorker(workItemArray);
            }
        }
    }
}
