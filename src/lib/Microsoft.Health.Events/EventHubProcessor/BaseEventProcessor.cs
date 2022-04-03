// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using EnsureThat;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Events.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public abstract class BaseEventProcessor
    {
        public BaseEventProcessor(
            EventProcessorClient eventProcessorClient,
            IEventConsumerService eventConsumerService,
            ICheckpointClient checkpointClient,
            ITelemetryLogger logger)
        {
            EventConsumerService = EnsureArg.IsNotNull(eventConsumerService, nameof(eventConsumerService));
            CheckpointClient = EnsureArg.IsNotNull(checkpointClient, nameof(checkpointClient));
            EventProcessorClient = EnsureArg.IsNotNull(eventProcessorClient, nameof(eventProcessorClient));
            Logger = EnsureArg.IsNotNull(logger, nameof(logger));
        }

        protected IEventConsumerService EventConsumerService { get; }

        protected ICheckpointClient CheckpointClient { get; }

        protected ITelemetryLogger Logger { get; }

        protected EventProcessorClient EventProcessorClient { get; }

        public abstract Task RunAsync(CancellationToken ct);

        // Processes two types of events
        // 1) Event hub events
        // 2) Maximum wait events. These are generated when we have not received an event hub
        //    event for a certain time period and this event is used to flush events in the current window.
        protected virtual async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            if (eventArgs.CancellationToken.IsCancellationRequested)
            {
                // The event arguments contain a cancellation token that the EventProcessorClient uses to signal the handler that processing should cease as soon as possible.
                // This is most commonly seen when the EventProcessorClient is stopping or has encountered an unrecoverable problem.
                Logger.LogTrace($"ProcessEventArgs contain a cancellation request {eventArgs.Partition.PartitionId}");
                return;
            }

            IEventMessage evt;
            if (eventArgs.HasEvent)
            {
                evt = EventMessageFactory.CreateEvent(eventArgs);
            }
            else
            {
                evt = new MaximumWaitEvent(eventArgs.Partition.PartitionId, DateTime.UtcNow);
            }

            await EventConsumerService.ConsumeEvent(evt);
        }

        protected virtual Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            EventHubExceptionProcessor.ProcessException(eventArgs.Exception, Logger);

            return Task.CompletedTask;
        }

        protected virtual async Task ProcessInitializingHandler(PartitionInitializingEventArgs initArgs)
        {
            var partitionId = initArgs.PartitionId;
            Logger.LogTrace($"Initializing partition {partitionId}");

            if (initArgs.CancellationToken.IsCancellationRequested)
            {
                // Log the condition where the initialization handler is called, an the PartitionInitializingEventArgs contains a cancellation request.
                Logger.LogTrace($"PartitionInitializingEventArgs contain a cancellation request {partitionId}");
            }

            try
            {
                var checkpoint = await CheckpointClient.GetCheckpointForPartitionAsync(partitionId, initArgs.CancellationToken);
                initArgs.DefaultStartingPosition = EventPosition.FromEnqueuedTime(checkpoint.LastProcessed);
                Logger.LogTrace($"Starting to read partition {partitionId} from checkpoint {checkpoint.LastProcessed}");
                Logger.LogMetric(EventMetrics.EventHubPartitionInitialized(partitionId), 1);
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Logger.LogTrace($"Failed to initialize partition {partitionId} from checkpoint");

                EventHubExceptionProcessor.ProcessException(ex, Logger, errorMetricName: EventHubErrorCode.EventHubPartitionInitFailed.ToString());
            }
        }
    }
}