// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementEventNormalizationServiceTests
    {
        private IContentTemplate _template;
        private IEnumerableAsyncCollector<IMeasurement> _consumer;
        private IConverter<EventData, JToken> _converter;
        private ITelemetryLogger _logger;
        private IExceptionTelemetryProcessor _exceptionTelemetryProcessor;

        public MeasurementEventNormalizationServiceTests()
        {
            _template = Substitute.For<IContentTemplate>();
            _consumer = Substitute.For<IEnumerableAsyncCollector<IMeasurement>>();
            _converter = Substitute.For<IConverter<EventData, JToken>>();
            _logger = Substitute.For<ITelemetryLogger>();
            _exceptionTelemetryProcessor = Substitute.For<IExceptionTelemetryProcessor>();
        }

        [Fact]
        public async Task GivenMultipleEventsWithOneResultPer_WhenProcessAsync_ThenEachEventConsumed_Test()
        {
            _template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>() });
            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            var srv = new MeasurementEventNormalizationService(_logger, _template, _exceptionTelemetryProcessor);
            await srv.ProcessAsync(events, _consumer);

            _template.ReceivedWithAnyArgs(events.Length).GetMeasurements(null);
            await _consumer.Received(1).AddAsync(Arg.Is<IEnumerable<IMeasurement>>(l => l.Count() == 10), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GivenMultipleEventsWithOneResultPer_WhenTotalMeauresmentsProcessed_AreGreaterThanBatchSize_ThenEachEventConsumedInMultipleBatches_Test()
        {
            _template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>() });
            var events = Enumerable.Range(0, 51).Select(i => BuildEvent(i)).ToArray();

            var srv = new MeasurementEventNormalizationService(_logger, _template, _converter, _exceptionTelemetryProcessor, 3, 25);
            await srv.ProcessAsync(events, _consumer);

            _template.ReceivedWithAnyArgs(events.Length).GetMeasurements(null);

            // 51 Events with a batch size of 25 =  2 * 25 measurement batches and 1 * 1 measurement batch
            await _consumer.Received(2).AddAsync(Arg.Is<IEnumerable<IMeasurement>>(l => l.Count() == 25), Arg.Any<CancellationToken>());
            await _consumer.Received(1).AddAsync(Arg.Is<IEnumerable<IMeasurement>>(l => l.Count() == 1), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GivenEventWithTwoResultPer_WhenProcessAsync_ThenEachEventResultConsumedTwice_Test()
        {
            var events = Enumerable.Range(0, 10)
                   .Select(i => BuildEvent(i))
                   .ToDictionary(ed => ed, ed => JToken.FromObject(new object()));

            _template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>(), Substitute.For<Measurement>() });
            _converter.Convert(null).ReturnsForAnyArgs(args => events[args.Arg<EventData>()]);

            var srv = new MeasurementEventNormalizationService(_logger, _template, _converter, _exceptionTelemetryProcessor, 3);
            await srv.ProcessAsync(events.Keys, _consumer);

            _template.ReceivedWithAnyArgs(events.Count).GetMeasurements(null);
            _converter.ReceivedWithAnyArgs(events.Count).Convert(null);
            await _consumer.Received(1).AddAsync(Arg.Is<IEnumerable<IMeasurement>>(l => l.Count() == events.Count * 2), Arg.Any<CancellationToken>());

            foreach (var evt in events)
            {
                _converter.Received(1).Convert(evt.Key);
                _template.Received(1).GetMeasurements(evt.Value);
            }
        }

        [Fact]
        public async Task GivenEvents_WhenProcessAsync_ThenIngestionTimeUtcSet_Test()
        {
            _template.GetMeasurements(null)
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

            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            var srv = new MeasurementEventNormalizationService(_logger, _template, _converter, _exceptionTelemetryProcessor, 1);
            await srv.ProcessAsync(events, _consumer);

            _template.ReceivedWithAnyArgs(10).GetMeasurements(null);
            _converter.ReceivedWithAnyArgs(10).Convert(null);
            await _consumer.Received(1).AddAsync(
                Arg.Is<IEnumerable<IMeasurement>>(
                    measurements =>
                        measurements.Count() == 10 &&
                        measurements.Where(m => events.Any(e => m.IngestionTimeUtc == e.SystemProperties.EnqueuedTimeUtc)).Count() == 10),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GivenEventsAndDefaultErrorConsumer_WhenProcessAsyncAndConsumerErrors_ThenEachEventResultConsumed_And_ErrorPerBatchProprogated_Test()
        {
            _template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>() });

            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            _consumer.AddAsync(null).ReturnsForAnyArgs(v => Task.FromException(new Exception()));

            var srv = new MeasurementEventNormalizationService(_logger, _template, _converter, _exceptionTelemetryProcessor, 1, 5); // Set asyncCollectorBatchSize to 5 to produce 2 batches
            var exception = await Assert.ThrowsAsync<AggregateException>(() => srv.ProcessAsync(events, _consumer));
            Assert.Equal(2, exception.InnerExceptions.Count);

            _template.ReceivedWithAnyArgs(events.Length).GetMeasurements(null);
            _converter.ReceivedWithAnyArgs(events.Length).Convert(null);
            await _consumer.Received(2).AddAsync(Arg.Is<IEnumerable<IMeasurement>>(l => l.Count() == 5), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GivenEventsAndDefaultErrorConsumer_WhenProcessAsyncAndHandleableConsumerErrors_ThenExceptionNotThrown_Test()
        {
            _template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>() });

            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            _converter.Convert(null).ReturnsForAnyArgs(v => throw new IncompatibleDataException());

            _exceptionTelemetryProcessor = new NormalizationExceptionTelemetryProcessor();

            var srv = new MeasurementEventNormalizationService(_logger, _template, _converter, _exceptionTelemetryProcessor, 1);
            await srv.ProcessAsync(events, _consumer);

            _template.ReceivedWithAnyArgs(0).GetMeasurements(null);
            _converter.ReceivedWithAnyArgs(events.Length).Convert(null);
            await _consumer.ReceivedWithAnyArgs(0).AddAsync(null);
        }

        [Fact]
        public async Task GivenEventsAndOperationCancellationException_WhenProcessAsync_ThenExecutionHalted_Test()
        {
            _template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>(), Substitute.For<Measurement>() });

            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            _consumer.AddAsync(null).ReturnsForAnyArgs(v => Task.FromException(new OperationCanceledException()));

            var srv = new MeasurementEventNormalizationService(_logger, _template, _converter, _exceptionTelemetryProcessor, 1);
            var exception = await Assert.ThrowsAsync<TaskCanceledException>(() => srv.ProcessAsync(events, _consumer));

            _template.ReceivedWithAnyArgs(events.Length).GetMeasurements(null);
            _converter.ReceivedWithAnyArgs(events.Length).Convert(null);
            await _consumer.Received(1).AddAsync(Arg.Is<IEnumerable<IMeasurement>>(l => l.Count() == events.Length * 2), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GivenEventsAndTaskCancellationException_WhenProcessAsync_ThenExecutionHalted_Test()
        {
            _template.GetMeasurements(null).ReturnsForAnyArgs(new[] { Substitute.For<Measurement>(), Substitute.For<Measurement>() });

            var events = Enumerable.Range(0, 10).Select(i => BuildEvent(i)).ToArray();

            _consumer.AddAsync(null).ReturnsForAnyArgs(v => Task.FromException(new TaskCanceledException()));

            var srv = new MeasurementEventNormalizationService(_logger, _template, _converter, _exceptionTelemetryProcessor, 1);
            var exception = await Assert.ThrowsAsync<TaskCanceledException>(() => srv.ProcessAsync(events, _consumer));

            _template.ReceivedWithAnyArgs(events.Length).GetMeasurements(null);
            _converter.ReceivedWithAnyArgs(events.Length).Convert(null);
            await _consumer.Received(1).AddAsync(Arg.Is<IEnumerable<IMeasurement>>(l => l.Count() == events.Length * 2), Arg.Any<CancellationToken>());
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
