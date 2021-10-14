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
    public class EventProcessor
    {
        private IEventConsumerService _eventConsumerService;
        private ICheckpointClient _checkpointClient;
        private ITelemetryLogger _logger;

        public EventProcessor(IEventConsumerService eventConsumerService, ICheckpointClient checkpointClient, ITelemetryLogger logger)
        {
            _eventConsumerService = eventConsumerService;
            _checkpointClient = checkpointClient;
            _logger = logger;
        }

        public async Task RunAsync(EventProcessorClient processor, CancellationToken ct)
        {
            EnsureArg.IsNotNull(processor);

            // Reset previous checkpoints corresponding to an older source event hub (i.e. applicable if the source event hub changes)
            await _checkpointClient.ResetCheckpointsAsync();

            // Processes two types of events
            // 1) Event hub events
            // 2) Maximum wait events. These are generated when we have not received an event hub
            //    event for a certain time period and this event is used to flush events in the current window.
            async Task ProcessEventHandler(ProcessEventArgs eventArgs)
            {
                IEventMessage evt;
                if (eventArgs.HasEvent)
                {
                    evt = EventMessageFactory.CreateEvent(eventArgs);
                }
                else
                {
                    evt = new MaximumWaitEvent(eventArgs.Partition.PartitionId, DateTime.UtcNow);
                }

                await _eventConsumerService.ConsumeEvent(evt);
            }

            // todo: consider retry
            Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
            {
                EventHubExceptionTelemetryProcessor.ProcessException(eventArgs.Exception, _logger);

                return Task.CompletedTask;
            }

            async Task ProcessInitializingHandler(PartitionInitializingEventArgs initArgs)
            {
                var partitionId = initArgs.PartitionId;
                _logger.LogTrace($"Initializing partition {partitionId}");

                try
                {
                    var checkpoint = await _checkpointClient.GetCheckpointForPartitionAsync(partitionId);
                    initArgs.DefaultStartingPosition = EventPosition.FromEnqueuedTime(checkpoint.LastProcessed);
                    _logger.LogTrace($"Starting to read partition {partitionId} from checkpoint {checkpoint.LastProcessed}");
                    _logger.LogMetric(EventMetrics.EventHubPartitionInitialized(partitionId), 1);
                }
#pragma warning disable CA1031
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    _logger.LogTrace($"Failed to initialize partition {partitionId} from checkpoint");
                    EventHubExceptionTelemetryProcessor.ProcessException(ex, _logger, errorMetricName: EventHubErrorCode.EventHubPartitionInitFailed.ToString());
                }
            }

            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;
            processor.PartitionInitializingAsync += ProcessInitializingHandler;

            try
            {
                Console.WriteLine($"Starting event hub processor at {DateTime.UtcNow}");
                await processor.StartProcessingAsync(ct);

                // Wait indefinitely until cancellation is requested
                ct.WaitHandle.WaitOne();

                await processor.StopProcessingAsync();
            }
            finally
            {
                processor.ProcessEventAsync -= ProcessEventHandler;
                processor.ProcessErrorAsync -= ProcessErrorHandler;
                processor.PartitionInitializingAsync -= ProcessInitializingHandler;
            }
        }
    }
}
