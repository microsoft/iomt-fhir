// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Events.UnitTest
{
    public class EventConsumerServiceTests
    {
        [Fact]
        public async void GivenEventConsumer_WhenEventConsumerThrowsException_ThenExceptionIsLogged_()
        {
            var initialBatch = new List<EventMessage>()
            {
                new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()))
            };

            var logger = Substitute.For<ITelemetryLogger>();
            var eventConsumer = Substitute.For<IEventConsumer>();

            eventConsumer.ConsumeAsync(Arg.Any<IEnumerable<IEventMessage>>()).ReturnsForAnyArgs(Task.FromException(new Exception("failure")));

            var eventEventConsumers = new List<IEventConsumer>() { eventConsumer };
            var eventConsumerService = new EventConsumerService(eventEventConsumers, logger);

            await eventConsumerService.ConsumeEvents(initialBatch);
            logger.Received(1).LogError(Arg.Is<Exception>(ex => ex.Message == "failure"));
            await eventConsumer.Received(1).ConsumeAsync(initialBatch);
        }
    }
}
