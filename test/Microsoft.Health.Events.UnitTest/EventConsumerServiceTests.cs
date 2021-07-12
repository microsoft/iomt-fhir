// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using Xunit;

namespace Microsoft.Health.Events.UnitTest
{
    public class EventConsumerServiceTests
    {
        [Fact]
        public async void GivenEventConsumer_WhenEventConsumerThrowsNonRetryableException_ThenEventConsumer_ThenIsNotRetried_()
        {
            var retries = 0;
            var initialBatch = new List<EventMessage>()
            {
                new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()))
            };

            var logger = Substitute.For<ITelemetryLogger>();
            var eventConsumer = Substitute.For<IEventConsumer>();

            eventConsumer.When(x => x.ConsumeAsync(initialBatch))
                .Do(x => { if (retries < 3) { retries++;  throw new Exception("failure"); } });

            var eventEventConsumers = new List<IEventConsumer>() { eventConsumer };
            var eventConsumerService = new EventConsumerService(eventEventConsumers, logger);

            await eventConsumerService.ConsumeEvents(initialBatch);
            logger.Received(1).LogError(Arg.Is<Exception>(ex => ex.Message == "failure"));
            await eventConsumer.Received(1).ConsumeAsync(initialBatch);
        }

        [Fact]
        public async void GivenEventConsumer_And_ConfiguredToNotRetry_WhenEventConsumerThrowsRetryableException_ThenIsNotRetried()
        {
            var retries = 0;
            var initialBatch = new List<EventMessage>()
            {
                new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()))
            };

            var logger = Substitute.For<ITelemetryLogger>();
            var eventConsumer = Substitute.For<IEventConsumer>();
            var exceptionProcessor = Substitute.For<Action<Exception>>();

            // fail to consume events 2 times in a row, then succeed
            eventConsumer.When(x => x.ConsumeAsync(initialBatch))
                .Do(x => { if (retries < 2) { retries++; throw new HttpRequestException("failure"); } });

            var eventEventConsumers = new List<IEventConsumer>() { eventConsumer };
            var eventConsumerService = new EventConsumerService(eventEventConsumers, logger, false, exceptionProcessor);

            // 2 retries followed by a successful call
            await eventConsumerService.ConsumeEvents(initialBatch);
            logger.Received(0).LogError(Arg.Is<Exception>(ex => ex.Message.StartsWith("Encountered retryable exception")));
            await eventConsumer.Received(1).ConsumeAsync(initialBatch);
            exceptionProcessor.Received(1).Invoke(Arg.Any<HttpRequestException>());
        }

        [Fact]
        public async void GivenEventConsumer_WhenEventConsumerThrowsRetryableException_ThenRetryEventConsumer_Test()
        {
            var retries = 0;
            var initialBatch = new List<EventMessage>()
            {
                new EventMessage("0", new ReadOnlyMemory<byte>(), 1, 1, new DateTime(2020, 12, 31, 5, 10, 20), new Dictionary<string, object>(), new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()))
            };

            var logger = Substitute.For<ITelemetryLogger>();
            var eventConsumer = Substitute.For<IEventConsumer>();
            var exceptionProcessor = Substitute.For<Action<Exception>>();

            // fail to consume events 2 times in a row, then succeed
            eventConsumer.When(x => x.ConsumeAsync(initialBatch))
                .Do(x => { if (retries < 2) { retries++; throw new HttpRequestException("failure"); } });

            var eventEventConsumers = new List<IEventConsumer>() { eventConsumer };
            var eventConsumerService = new EventConsumerService(eventEventConsumers, logger, true, exceptionProcessor);

            // 2 retries followed by a successful call
            await eventConsumerService.ConsumeEvents(initialBatch);
            logger.Received(2).LogError(Arg.Is<Exception>(ex => ex.Message.StartsWith("Encountered retryable exception")));
            await eventConsumer.Received(3).ConsumeAsync(initialBatch);
            exceptionProcessor.Received(2).Invoke(Arg.Any<HttpRequestException>());
        }
    }
}
