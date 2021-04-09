// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementEventNormalizationServiceTests
    {
        [Fact]
        public async Task GivenMultipleEventsWithOneResultPer_WhenProcessAsync_ThenEachEventConsumed_Test()
        {
            var template = Substitute.For<IContentTemplate>();
            template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>() });
            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            var log = Substitute.For<ITelemetryLogger>();

            var consumer = Substitute.For<IAsyncCollector<IMeasurement>>();

            var srv = new MeasurementEventNormalizationService(log, template);
            await srv.ProcessAsync(events, consumer);

            template.ReceivedWithAnyArgs(events.Length).GetMeasurements(null);
            await consumer.ReceivedWithAnyArgs(events.Length).AddAsync(null);
        }

        [Fact]
        public async Task GivenEventWithTwoResultPer_WhenProcessAsync_ThenEachEventResultConsumedTwice_Test()
        {
            var events = Enumerable.Range(0, 10)
                   .Select(i => BuildEvent(i))
                   .ToDictionary(ed => ed, ed => JToken.FromObject(new object()));

            var template = Substitute.For<IContentTemplate>();
            template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>(), Substitute.For<Measurement>() });
            var converter = Substitute.For<Data.IConverter<EventData, JToken>>();
            converter.Convert(null).ReturnsForAnyArgs(args => events[args.Arg<EventData>()]);

            var log = Substitute.For<ITelemetryLogger>();

            var consumer = Substitute.For<IAsyncCollector<IMeasurement>>();

            var srv = new MeasurementEventNormalizationService(log, template, converter, 3);
            await srv.ProcessAsync(events.Keys, consumer);

            template.ReceivedWithAnyArgs(events.Count).GetMeasurements(null);
            converter.ReceivedWithAnyArgs(events.Count).Convert(null);
            await consumer.ReceivedWithAnyArgs(events.Count * 2).AddAsync(null);

            foreach (var evt in events)
            {
                converter.Received(1).Convert(evt.Key);
                template.Received(1).GetMeasurements(evt.Value);
            }
        }

        [Fact]
        public async Task GivenEvents_WhenProcessAsync_ThenIngestionTimeUtcSet_Test()
        {
            var template = Substitute.For<IContentTemplate>();
            template.GetMeasurements(null)
                .ReturnsForAnyArgs(
                    new[] { new Measurement() },
                    new[] { new Measurement() },
                    new[] { new Measurement() },
                    new[] { new Measurement() },
                    new[] { new Measurement() },
                    new[] { new Measurement() },
                    new[] { new Measurement() },
                    new[] { new Measurement() },
                    new[] { new Measurement() },
                    new[] { new Measurement() });

            var converter = Substitute.For<Data.IConverter<EventData, JToken>>();

            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            var log = Substitute.For<ITelemetryLogger>();

            var consumer = Substitute.For<IAsyncCollector<IMeasurement>>();

            var srv = new MeasurementEventNormalizationService(log, template, converter, 1);
            await srv.ProcessAsync(events, consumer);

            template.ReceivedWithAnyArgs(10).GetMeasurements(null);
            converter.ReceivedWithAnyArgs(10).Convert(null);
            await consumer.ReceivedWithAnyArgs(10).AddAsync(null);

            foreach (var evt in events)
            {
                await consumer.Received(1)
                    .AddAsync(Arg.Is<Measurement>(m => m.IngestionTimeUtc == evt.SystemProperties.EnqueuedTimeUtc));
            }
        }

        [Fact]
        public async Task GivenEventsAndDefaultErrorConsumer_WhenProcessAsyncAndConsumerErrors_ThenEachEventResultConsumedAndErrorProprogated_Test()
        {
            var template = Substitute.For<IContentTemplate>();
            template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>() });
            var converter = Substitute.For<Data.IConverter<EventData, JToken>>();

            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            var log = Substitute.For<ITelemetryLogger>();

            var consumer = Substitute.For<IAsyncCollector<IMeasurement>>();
            consumer.AddAsync(null).ReturnsForAnyArgs(v => Task.FromException(new Exception()));

            var srv = new MeasurementEventNormalizationService(log, template, converter, 1);
            var exception = await Assert.ThrowsAsync<AggregateException>(() => srv.ProcessAsync(events, consumer));
            Assert.Equal(events.Length, exception.InnerExceptions.Count);

            template.ReceivedWithAnyArgs(events.Length).GetMeasurements(null);
            converter.ReceivedWithAnyArgs(events.Length).Convert(null);
            await consumer.ReceivedWithAnyArgs(events.Length).AddAsync(null);
        }

        [Fact]
        public async Task GivenEventsAndOperationCancellationException_WhenProcessAsync_ThenExecutionHalted_Test()
        {
            var template = Substitute.For<IContentTemplate>();
            template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>(), Substitute.For<Measurement>() });
            var converter = Substitute.For<Data.IConverter<EventData, JToken>>();

            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            var log = Substitute.For<ITelemetryLogger>();

            var consumer = Substitute.For<IAsyncCollector<IMeasurement>>();
            consumer.AddAsync(null).ReturnsForAnyArgs(v => Task.FromException(new OperationCanceledException()));

            var srv = new MeasurementEventNormalizationService(log, template, converter, 1);
            var exception = await Assert.ThrowsAsync<TaskCanceledException>(() => srv.ProcessAsync(events, consumer));

            template.ReceivedWithAnyArgs(1).GetMeasurements(null);
            converter.ReceivedWithAnyArgs(1).Convert(null);
            await consumer.ReceivedWithAnyArgs(1).AddAsync(null);
        }

        [Fact]
        public async Task GivenEventsAndTaskCancellationException_WhenProcessAsync_ThenExecutionHalted_Test()
        {
            var template = Substitute.For<IContentTemplate>();
            template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>(), Substitute.For<Measurement>() });
            var converter = Substitute.For<Data.IConverter<EventData, JToken>>();

            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            var log = Substitute.For<ITelemetryLogger>();

            var consumer = Substitute.For<IAsyncCollector<IMeasurement>>();
            consumer.AddAsync(null).ReturnsForAnyArgs(v => Task.FromException(new TaskCanceledException()));

            var srv = new MeasurementEventNormalizationService(log, template, converter, 1);
            var exception = await Assert.ThrowsAsync<TaskCanceledException>(() => srv.ProcessAsync(events, consumer));

            template.ReceivedWithAnyArgs(1).GetMeasurements(null);
            converter.ReceivedWithAnyArgs(1).Convert(null);
            await consumer.ReceivedWithAnyArgs(1).AddAsync(null);
        }

        private static EventData BuildEvent(int? sequence = 0)
        {
            return new EventData(Array.Empty<byte>())
            {
                SystemProperties = new EventData.SystemPropertiesCollection(sequence.Value, DateTime.UtcNow.AddSeconds(sequence.Value - 10), sequence?.ToString(), 0.ToString()),
            };
        }
    }
}
