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
        [Fact]
        public async void GivenBatchServiceCreated_WhenFirstEventReceived_ThenCreateQueueAndWindow_Test()
        {
            var eventConsumerService = Substitute.For<IEventConsumerService>();
            var checkpointClient = Substitute.For<ICheckpointClient>();
            var logger = Substitute.For<ITelemetryLogger>();
            var options = new EventBatchingOptions();
            options.FlushTimespan = 300;
            options.MaxEvents = 5;

            var eventReader = new EventBatchingService(eventConsumerService, options, checkpointClient, logger);

            var enqueuedTime = DateTime.UtcNow;

            var event1 = new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, enqueuedTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));

            await eventReader.ConsumeEvent(event1);

            var endWindow = enqueuedTime.Add(TimeSpan.FromSeconds(options.FlushTimespan));
            var partitionWindow = eventReader.GetPartition("0").GetPartitionWindow();

            Assert.Equal(endWindow, partitionWindow);
        }

        [Fact]
        public async void GivenBatchServiceCreated_WhenNextEventReadAndNoEventsInCurrentWindow_ThenAdvanceWindowToIncludeCurrentEvent_Test()
        {
            var eventConsumerService = Substitute.For<IEventConsumerService>();
            var checkpointClient = Substitute.For<ICheckpointClient>();
            var logger = Substitute.For<ITelemetryLogger>();
            var options = new EventBatchingOptions();
            options.FlushTimespan = 300;
            options.MaxEvents = 5;

            var eventReader = new EventBatchingService(eventConsumerService, options, checkpointClient, logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-901);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent);

            var endWindow = firstEventTime.Add(TimeSpan.FromSeconds(options.FlushTimespan));
            Assert.Equal(endWindow, eventReader.GetPartition("0").GetPartitionWindow());

            var nextEventTime = DateTime.UtcNow;
            var nextEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), 2, 2, nextEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(nextEvent);

            // check that the window is incremented up until next event is included in the current window
            var currentWindowStart = eventReader.GetPartition(nextEvent.PartitionId).GetPartitionWindow().Add(-TimeSpan.FromSeconds(options.FlushTimespan));
            var currrentWindowEnd = eventReader.GetPartition(nextEvent.PartitionId).GetPartitionWindow();

            Assert.InRange(nextEventTime, currentWindowStart, currrentWindowEnd);
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenEventReceivedOutsideWindow_ThenFlushEventsInQueueAndCreateNewWindow_Test()
        {
            var eventConsumerService = Substitute.For<IEventConsumerService>();
            var checkpointClient = Substitute.For<ICheckpointClient>();
            var logger = Substitute.For<ITelemetryLogger>();
            var options = new EventBatchingOptions();
            options.FlushTimespan = 300;
            options.MaxEvents = 5;

            var eventReader = new EventBatchingService(eventConsumerService, options, checkpointClient, logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-301);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent);

            // first window end is: utc - 1 second
            // utc - 301 seconds (firstEventTime) + 300 seconds (FlushTimespan)
            var endWindow = firstEventTime.Add(TimeSpan.FromSeconds(options.FlushTimespan));
            Assert.Equal(endWindow, eventReader.GetPartition(firstEvent.PartitionId).GetPartitionWindow());

            var nextEventTime = DateTime.UtcNow;
            var nextEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), 2, 2, nextEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(nextEvent);

            // flush the 1 event that exists within the first window, verify event outside of window is in queue
            await eventConsumerService.Received(1).ConsumeEvents(Arg.Is<IEnumerable<IEventMessage>>(x => x.Count() == 1));
            var expectedQueueCount = 1;
            Assert.Equal(expectedQueueCount, eventReader.GetPartition(firstEvent.PartitionId).GetPartitionBatchCount());

            // check that the window is incremented
            var newEndWindow = endWindow.Add(TimeSpan.FromSeconds(options.FlushTimespan));
            Assert.Equal(newEndWindow, eventReader.GetPartition(firstEvent.PartitionId).GetPartitionWindow());
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenMaxEventsReached_ThenFlushEventsAndKeepWindow_Test()
        {
            var eventConsumerService = Substitute.For<IEventConsumerService>();
            var checkpointClient = Substitute.For<ICheckpointClient>();
            var logger = Substitute.For<ITelemetryLogger>();
            var options = new EventBatchingOptions();
            options.FlushTimespan = 300;
            options.MaxEvents = 3;

            var eventReader = new EventBatchingService(eventConsumerService, options, checkpointClient, logger);

            var newEventTime = DateTime.UtcNow;
            var newEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, newEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(newEvent);
            await eventReader.ConsumeEvent(newEvent);

            var endWindow = newEventTime.Add(TimeSpan.FromSeconds(options.FlushTimespan));
            Assert.Equal(endWindow, eventReader.GetPartition(newEvent.PartitionId).GetPartitionWindow());

            await eventConsumerService.Received(0).ConsumeEvents(Arg.Any<IEnumerable<IEventMessage>>());

            await eventReader.ConsumeEvent(newEvent);
            await eventConsumerService.Received(1).ConsumeEvents(Arg.Any<IEnumerable<IEventMessage>>());

            Assert.Equal(endWindow, eventReader.GetPartition(newEvent.PartitionId).GetPartitionWindow());
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenFlushWindowHasPassedAndMaxWaitEventReceived_ThenFlush_Test()
        {
            var eventConsumerService = Substitute.For<IEventConsumerService>();
            var checkpointClient = Substitute.For<ICheckpointClient>();
            var logger = Substitute.For<ITelemetryLogger>();
            var options = new EventBatchingOptions();
            options.FlushTimespan = 300;
            options.MaxEvents = 5;

            var eventReader = new EventBatchingService(eventConsumerService, options, checkpointClient, logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-400);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent);

            var maxWaitEvent = new MaximumWaitEvent("0", DateTime.UtcNow.AddSeconds(-10));
            await eventReader.ConsumeEvent(maxWaitEvent);
            await eventConsumerService.Received(1).ConsumeEvents(Arg.Any<IEnumerable<IEventMessage>>());
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenFlushWindowHasNotPassedAndMaxWaitEventReceived_ThenDoNotFlush_Test()
        {
            var eventConsumerService = Substitute.For<IEventConsumerService>();
            var checkpointClient = Substitute.For<ICheckpointClient>();
            var logger = Substitute.For<ITelemetryLogger>();
            var options = new EventBatchingOptions();
            options.FlushTimespan = 300;
            options.MaxEvents = 5;

            var eventReader = new EventBatchingService(eventConsumerService, options, checkpointClient, logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-30);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent);

            var maxWaitEvent = new MaximumWaitEvent("0", DateTime.UtcNow.AddSeconds(-10));
            await eventReader.ConsumeEvent(maxWaitEvent);
            await eventConsumerService.Received(0).ConsumeEvents(Arg.Any<IEnumerable<EventMessage>>());
        }

        [Fact]
        public async void GivenBatchServiceRunning_WhenNewEventConsumed_ThenCheckpointUpdatedLocally_Test()
        {
            var eventConsumerService = Substitute.For<IEventConsumerService>();
            var checkpointClient = Substitute.For<ICheckpointClient>();
            var logger = Substitute.For<ITelemetryLogger>();
            var options = new EventBatchingOptions();
            options.FlushTimespan = 300;
            options.MaxEvents = 1;

            var eventReader = new EventBatchingService(eventConsumerService, options, checkpointClient, logger);

            var firstEventTime = DateTime.UtcNow.AddSeconds(-301);
            var firstEvent = new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, firstEventTime, new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()));
            await eventReader.ConsumeEvent(firstEvent);

            await checkpointClient.Received(1).SetCheckpointAsync(firstEvent);
        }
    }
}
