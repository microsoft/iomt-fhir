using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace Microsoft.Health.Events.UnitTest
{
    public class EventBatchingTests
    {
        private IEventConsumerService _eventConsumerService;
        private ICheckpointClient _checkpointClient;
        private ITelemetryLogger _logger;
        private EventBatchingOptions _options = new EventBatchingOptions();

        public EventBatchingTests()
        {
            _eventConsumerService = Substitute.For<IEventConsumerService>();
            _checkpointClient = Substitute.For<ICheckpointClient>();
            _logger = Substitute.For<ITelemetryLogger>();
            _options.FlushTimespan = 300;
            _options.MaxEvents = 5;
        }

        [Fact]
        public async void GivenBatchServiceCreated_WhenFirstEventReceived_ThenCreateQueueAndWindow_Test()
        {
            var eventReader = new EventBatchingService(_eventConsumerService, _options, _checkpointClient, _logger);

            var enqueuedTime = DateTime.UtcNow;

            var event1 = new EventMessage("0", new ReadOnlyMemory<byte>(), null, 1, 1, enqueuedTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));

            await eventReader.ConsumeEvent(event1, default);

            var endWindow = enqueuedTime.Add(TimeSpan.FromSeconds(_options.FlushTimespan));
            var partitionWindow = eventReader.GetPartition("0").GetPartitionWindow();

            Assert.Equal(endWindow, partitionWindow);
        }

        [Fact]
        public async void GivenBatchServiceCreated_WhenNextEventReadAndNoEventsInCurrentWindow_ThenAdvanceWindowToIncludeCurrentEvent_Test()
        {
            var eventReader = new EventBatchingService(_eventConsumerService, _options, _checkpointClient, _logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-901);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), null, 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent, default);

            var endWindow = firstEventTime.Add(TimeSpan.FromSeconds(_options.FlushTimespan));
            Assert.Equal(endWindow, eventReader.GetPartition("0").GetPartitionWindow());

            var nextEventTime = DateTime.UtcNow;
            var nextEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), null, 2, 2, nextEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(nextEvent, default);

            // check that the window is incremented up until next event is included in the current window
            var currentWindowStart = eventReader.GetPartition(nextEvent.PartitionId).GetPartitionWindow().Add(-TimeSpan.FromSeconds(_options.FlushTimespan));
            var currrentWindowEnd = eventReader.GetPartition(nextEvent.PartitionId).GetPartitionWindow();

            Assert.InRange(nextEventTime, currentWindowStart, currrentWindowEnd);
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenEventReceivedOutsideWindow_ThenFlushEventsInQueueAndCreateNewWindow_Test()
        {
            var eventReader = new EventBatchingService(_eventConsumerService, _options, _checkpointClient, _logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-301);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), null, 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent, default);

            // first window end is: utc - 1 second
            // utc - 301 seconds (firstEventTime) + 300 seconds (FlushTimespan)
            var endWindow = firstEventTime.Add(TimeSpan.FromSeconds(_options.FlushTimespan));
            Assert.Equal(endWindow, eventReader.GetPartition(firstEvent.PartitionId).GetPartitionWindow());

            var nextEventTime = DateTime.UtcNow;
            var nextEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), null, 2, 2, nextEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(nextEvent, default);

            // flush the 1 event that exists within the first window, verify event outside of window is in queue
            await _eventConsumerService.Received(1).ConsumeEvents(Arg.Is<IEnumerable<IEventMessage>>(x => x.Count() == 1), default);
            var expectedQueueCount = 1;
            Assert.Equal(expectedQueueCount, eventReader.GetPartition(firstEvent.PartitionId).GetPartitionBatchCount());

            // check that the window is incremented
            var newEndWindow = endWindow.Add(TimeSpan.FromSeconds(_options.FlushTimespan));
            Assert.Equal(newEndWindow, eventReader.GetPartition(firstEvent.PartitionId).GetPartitionWindow());
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenMaxEventsReached_ThenFlushEventsAndKeepWindow_Test()
        {
            _options.MaxEvents = 3;

            var eventReader = new EventBatchingService(_eventConsumerService, _options, _checkpointClient, _logger);

            var newEventTime = DateTime.UtcNow;
            var newEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), null, 1, 1, newEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(newEvent, default);
            await eventReader.ConsumeEvent(newEvent, default);

            var endWindow = newEventTime.Add(TimeSpan.FromSeconds(_options.FlushTimespan));
            Assert.Equal(endWindow, eventReader.GetPartition(newEvent.PartitionId).GetPartitionWindow());

            await _eventConsumerService.Received(0).ConsumeEvents(Arg.Any<IEnumerable<IEventMessage>>(), default);

            await eventReader.ConsumeEvent(newEvent, default);
            await _eventConsumerService.Received(1).ConsumeEvents(Arg.Any<IEnumerable<IEventMessage>>(), default);

            Assert.Equal(endWindow, eventReader.GetPartition(newEvent.PartitionId).GetPartitionWindow());
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenFlushWindowHasPassedAndMaxWaitEventReceived_ThenFlush_Test()
        {
            var eventReader = new EventBatchingService(_eventConsumerService, _options, _checkpointClient, _logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-400);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), null, 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent, default);

            var maxWaitEvent = new MaximumWaitEvent("0", DateTime.UtcNow.AddSeconds(-10));
            await eventReader.ConsumeEvent(maxWaitEvent, default);
            await _eventConsumerService.Received(1).ConsumeEvents(Arg.Any<IEnumerable<IEventMessage>>(), default);
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenFlushWindowHasNotPassedAndMaxWaitEventReceived_ThenDoNotFlush_Test()
        {
            var eventReader = new EventBatchingService(_eventConsumerService, _options, _checkpointClient, _logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-30);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), null, 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent, default);

            var maxWaitEvent = new MaximumWaitEvent("0", DateTime.UtcNow.AddSeconds(-10));
            await eventReader.ConsumeEvent(maxWaitEvent, default);
            await _eventConsumerService.Received(0).ConsumeEvents(Arg.Any<IEnumerable<EventMessage>>(), default);
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenNewEventConsumed_ThenCheckpointUpdatedLocally_Test()
        {
            _options.MaxEvents = 1;

            var eventReader = new EventBatchingService(_eventConsumerService, _options, _checkpointClient, _logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-301);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), null, 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent, default);

            await _checkpointClient.Received(1).SetCheckpointAsync(firstEvent, default);
        }

        [Fact]
        public async void GivenBatchServiceCreated_WhenPartitionAcquired_ThenPartitionQueueCleared_Test()
        {
            var partitionId = "0";
            var eventReader = new EventBatchingService(_eventConsumerService, _options, _checkpointClient, _logger);

            var enqueuedTime = DateTime.UtcNow;

            var event1 = new EventMessage(partitionId, new ReadOnlyMemory<byte>(), null, 1, 1, enqueuedTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(event1, default);

            var endWindow = enqueuedTime.Add(TimeSpan.FromSeconds(_options.FlushTimespan));
            var partitionWindow = eventReader.GetPartition(partitionId).GetPartitionWindow();

            Assert.Equal(endWindow, partitionWindow);

            eventReader.NewPartitionInitialized(partitionId);
            var partitionExists = eventReader.EventPartitionExists(partitionId);

            Assert.False(partitionExists);

            var enqueuedTime2 = DateTime.UtcNow;
            var event2 = new EventMessage(partitionId, new ReadOnlyMemory<byte>(), null, 1, 1, enqueuedTime2, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(event2, default);

            var endWindow2 = enqueuedTime2.Add(TimeSpan.FromSeconds(_options.FlushTimespan));
            var partitionWindow2 = eventReader.GetPartition(partitionId).GetPartitionWindow();

            Assert.Equal(endWindow2, partitionWindow2);
        }
    }
}
