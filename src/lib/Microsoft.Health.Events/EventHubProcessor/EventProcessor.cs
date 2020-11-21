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

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class EventProcessor
    {
        private IEventConsumerService _eventConsumerService;
        private StorageCheckpointClient _checkpointClient;

        public EventProcessor(IEventConsumerService eventConsumerService, StorageCheckpointClient checkpointClient)
        {
            _eventConsumerService = eventConsumerService;
            _checkpointClient = checkpointClient;
        }

        public async Task RunAsync(EventProcessorClient processor, CancellationToken ct)
        {
            EnsureArg.IsNotNull(processor);

            Task ProcessEventHandler(ProcessEventArgs eventArgs)
            {
                try
                {
                    if (eventArgs.HasEvent)
                    {
                        var evt = new Event(
                            eventArgs.Partition.PartitionId,
                            eventArgs.Data.Body,
                            eventArgs.Data.Offset,
                            eventArgs.Data.SequenceNumber,
                            eventArgs.Data.EnqueuedTime.UtcDateTime,
                            eventArgs.Data.SystemProperties);

                        _eventConsumerService.ConsumeEvent(evt);
                    }
                    else
                    {
                        var evt = new MaximumWaitEvent(eventArgs.Partition.PartitionId, DateTime.UtcNow);
                        _eventConsumerService.ConsumeEvent(evt);
                    }
                }
                catch
                {
                    throw;
                }

                return Task.CompletedTask;
            }

            Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
            {
                // todo: add an error processor
                Console.WriteLine(eventArgs.Exception.Message);
                return Task.CompletedTask;
            }

            async Task ProcessInitializingHandler(PartitionInitializingEventArgs initArgs)
            {
                Console.WriteLine($"Initializing partition {initArgs.PartitionId}");

                var partitionId = initArgs.PartitionId;

                // todo: only get checkpoint for partition instead of listing them all
                var checkpoints = await _checkpointClient.ListCheckpointsAsync();
                foreach (var checkpoint in checkpoints)
                {
                    if (checkpoint.Id == partitionId)
                    {
                        initArgs.DefaultStartingPosition = EventPosition.FromEnqueuedTime(checkpoint.LastProcessed);
                        Console.WriteLine($"Starting to read partition {partitionId} from checkpoint {checkpoint.LastProcessed}");
                        break;
                    }
                }
            }

            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;
            processor.PartitionInitializingAsync += ProcessInitializingHandler;

            try
            {
                Console.WriteLine($"Starting event hub processor at {DateTime.UtcNow}");
                await processor.StartProcessingAsync();

                while (!ct.IsCancellationRequested)
                {
                }

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
