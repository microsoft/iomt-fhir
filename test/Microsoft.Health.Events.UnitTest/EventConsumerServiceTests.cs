using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Microsoft.Health.Events.UnitTest
{
    public class EventConsumerServiceTests
    {
        [Fact]
        public async void GivenEventConsumer_WhenEventConsumerThrowsException_ThenRetryEventConsumer_Test()
        {
            var retries = 0;
            var initialBatch = new List<EventMessage>()
            {
                new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()))
            };

            var logger = Substitute.For<ITelemetryLogger>();
            var eventConsumer = Substitute.For<IEventConsumer>();

            // fail to consume events 3 times in a row, then succeed
            eventConsumer.When(x => x.ConsumeAsync(initialBatch))
                .Do(x => { if (retries < 3) { retries++;  throw new Exception("failure"); } });

            var eventEventConsumers = new List<IEventConsumer>() { eventConsumer };
            var eventConsumerService = new EventConsumerService(eventEventConsumers, logger);

            // 3 retries followed by a successful call
            await eventConsumerService.ConsumeEvents(initialBatch);
            await eventConsumer.Received(4).ConsumeAsync(initialBatch);
        }
    }
}
