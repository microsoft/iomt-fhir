// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class MeasurementToEventMessageAsyncCollectorTests
    {
        private IEventHubMessageService _eventHubService;
        private IHashCodeFactory _hashCodeFactory;
        private IHashCodeGenerator _hashCodeGenerator;
        private ITelemetryLogger _telemetryLogger;
        private IEnumerableAsyncCollector<IMeasurement> _measurementCollector;

        public MeasurementToEventMessageAsyncCollectorTests()
        {
            _eventHubService = Substitute.For<IEventHubMessageService>();
            _hashCodeFactory = Substitute.For<IHashCodeFactory>();
            _telemetryLogger = Substitute.For<ITelemetryLogger>();

            _measurementCollector = new MeasurementToEventMessageAsyncCollector(_eventHubService, _hashCodeFactory, _telemetryLogger);
            _hashCodeGenerator = Substitute.For<IHashCodeGenerator>();
            _hashCodeGenerator.GenerateHashCode(Arg.Any<string>()).Returns("123");
            _hashCodeFactory.CreateDeterministicHashCodeGenerator().Returns(_hashCodeGenerator);
        }

        [Fact]
        public async void GivenMeasurementWithDeviceId_WhenAddAsync_ThenEventSentWithDeviceIdAsPartitionKey_Test()
        {
            var measurement = Substitute.For<IMeasurement>();
            measurement.DeviceId.Returns("1");

            await _measurementCollector.AddAsync(measurement).ConfigureAwait(false);

            await _eventHubService.Received(1)
                .SendAsync(
                    Arg.Is<IEnumerable<EventData>>(
                        data =>
                            data.Count() == 1 && data.First().EventBody.ToObjectFromJson<Measurement>(null).DeviceId == measurement.DeviceId),
                    "1",
                    default);
        }

        [Fact]
        public async void GivenMeasurementWithOutDeviceId_WhenAddAsync_ThenArgumentExceptionThrown_Test()
        {
            var measurement = Substitute.For<IMeasurement>();

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _measurementCollector.AddAsync(measurement).ConfigureAwait(false));
            Assert.Contains("DeviceId", ex.Message);
        }

        [Fact]
        public async void GivenCollectionOfMeasurements_WhenAddAsync_ThenAllEventsAreSentInASingleBatch_Test()
        {
            var mockEventDataBatch = EventHubsModelFactory.EventDataBatch(
                10000,
                new List<EventData>(),
                new CreateBatchOptions()
                {
                    PartitionKey = "partition123",
                },
                (data) => true);

            _eventHubService.CreateEventDataBatchAsync(Arg.Any<string>()).Returns(mockEventDataBatch);

            var measurements = Enumerable.Range(0, 100).Select(i =>
             {
                 var mockMeasurement = Substitute.For<IMeasurement>();
                 mockMeasurement.DeviceId.Returns($"deviceId_{i}");
                 return mockMeasurement;
             });

            await _measurementCollector.AddAsync(measurements, default);
            await _eventHubService.Received(1).CreateEventDataBatchAsync("123");
            await _eventHubService.Received(1)
                .SendAsync(
                    Arg.Is<EventDataBatch>(data => data.Count == 100),
                    default);
        }

        [Fact]
        public async void GivenCollectionOfMeasurements_WhenAddAsync_AndEventsCannotFitInSingleBatch_ThenEventsAreSentInAMultipeBatches_Test()
        {
            var count = 0;
            var simpleMockEventDataBatch = EventHubsModelFactory.EventDataBatch(
                10000,
                new List<EventData>(),
                new CreateBatchOptions()
                {
                    PartitionKey = "partition123",
                });
            var splittingEventDataBatch = EventHubsModelFactory.EventDataBatch(
                10000,
                new List<EventData>(),
                new CreateBatchOptions()
                {
                    PartitionKey = "partition123",
                },
                (data) => count++ != 5); // split at 5 measurement

            _eventHubService.CreateEventDataBatchAsync(Arg.Any<string>())
                .Returns(splittingEventDataBatch, simpleMockEventDataBatch);

            var measurements = Enumerable.Range(0, 10).Select(i =>
            {
                var mockMeasurement = Substitute.For<IMeasurement>();
                mockMeasurement.DeviceId.Returns($"deviceId_{i}");
                return mockMeasurement;
            });

            await _measurementCollector.AddAsync(measurements, default);

            await _eventHubService.Received(2).CreateEventDataBatchAsync("123");
            await _eventHubService.Received(2)
                .SendAsync(
                    Arg.Is<EventDataBatch>(data => data.Count == 5),
                    default);
        }

        [Fact]
        public async void GivenCollectionOfMeasurements_WhenAddAsync_AndAEventIsToBigToSend_ThenEventsIsSkipped_Test()
        {
            var eventDataBatch = EventHubsModelFactory.EventDataBatch(
                 10000,
                 new List<EventData>(),
                 new CreateBatchOptions()
                 {
                     PartitionKey = "partition123",
                 },
                 (data) =>
                 {
                     var measurement = data.EventBody.ToObjectFromJson<Measurement>(null);
                     return measurement.DeviceId != "deviceId_5";
                 });

            _eventHubService.CreateEventDataBatchAsync(Arg.Any<string>()).Returns(eventDataBatch);

            var measurements = Enumerable.Range(0, 10).Select(i =>
            {
                var mockMeasurement = Substitute.For<IMeasurement>();
                mockMeasurement.DeviceId.Returns($"deviceId_{i}");
                return mockMeasurement;
            });

            await _measurementCollector.AddAsync(measurements, default);

            await _eventHubService.Received(2).CreateEventDataBatchAsync("123");

            await _eventHubService.Received(1)
                .SendAsync(
                    Arg.Is<EventDataBatch>(data => data.Count == 9),
                    default);
        }

        [Fact]
        public async void GivenCollector_WhenFlushAsync_OperationSuccess_Test()
        {
            await _measurementCollector.FlushAsync();
        }
    }
}
