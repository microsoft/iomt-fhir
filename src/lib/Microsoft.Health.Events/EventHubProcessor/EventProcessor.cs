// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class EventProcessor : BaseEventProcessor
    {
        public EventProcessor(EventProcessorClient processor, IEventConsumerService eventConsumerService, ICheckpointClient checkpointClient, ITelemetryLogger logger)
            : base(processor, eventConsumerService, checkpointClient, logger)
        {
        }

        public override async Task RunAsync(CancellationToken ct)
        {
            // Reset previous checkpoints corresponding to an older source event hub (i.e. applicable if the source event hub changes)
            await CheckpointClient.ResetCheckpointsAsync(ct);

            EventProcessorClient.ProcessEventAsync += ProcessEventHandler;
            EventProcessorClient.ProcessErrorAsync += ProcessErrorHandler;
            EventProcessorClient.PartitionInitializingAsync += ProcessInitializingHandler;

            try
            {
                Console.WriteLine($"Starting event hub processor at {DateTime.UtcNow}");
                await EventProcessorClient.StartProcessingAsync(ct);

                // Wait indefinitely until cancellation is requested
                ct.WaitHandle.WaitOne();

                await EventProcessorClient.StopProcessingAsync();
            }
            finally
            {
                EventProcessorClient.ProcessEventAsync -= ProcessEventHandler;
                EventProcessorClient.ProcessErrorAsync -= ProcessErrorHandler;
                EventProcessorClient.PartitionInitializingAsync -= ProcessInitializingHandler;
            }
        }
    }
}
