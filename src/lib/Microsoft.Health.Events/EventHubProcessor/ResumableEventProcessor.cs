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
    public class ResumableEventProcessor : BaseEventProcessor, IResumableEventProcessor
    {
        private bool _isDisposed;
        private bool _isInitialized;

        public ResumableEventProcessor(
            EventProcessorClient processor,
            IEventConsumerService eventConsumerService,
            ICheckpointClient checkpointClient,
            ITelemetryLogger logger)
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
            EventProcessorClient.PartitionClosingAsync += PartitionClosingHandler;

            _isInitialized = true;

            await ResumeAsync(ct);
        }

        public async Task ResumeAsync(CancellationToken ct)
        {
            // Only start if necessary.
            if (!EventProcessorClient.IsRunning)
            {
                try
                {
                    Logger.LogTrace($"Starting event hub processor at {DateTime.UtcNow}");
                    await EventProcessorClient.StartProcessingAsync(ct);
                }
                catch (AggregateException ex)
                {
                    HandleOwnershipFailure(ex);
                }
            }
            else
            {
                Logger.LogTrace("Eventhub Processor is already running");
            }
        }

        public async Task SuspendAsync(CancellationToken ct)
        {
            if (EventProcessorClient.IsRunning)
            {
                Logger.LogTrace($"Suspending Eventhub Processor at {DateTime.UtcNow}");
                await Shutdown(ct);
            }
            else
            {
                Logger.LogTrace("Eventhub Processor is already suspended");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async Task Shutdown(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace($"Stopping event hub processor at {DateTime.UtcNow}");
            await EventProcessorClient.StopProcessingAsync(cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Shutdown().Wait();

                if (_isInitialized)
                {
                    EventProcessorClient.ProcessEventAsync -= ProcessEventHandler;
                    EventProcessorClient.ProcessErrorAsync -= ProcessErrorHandler;
                    EventProcessorClient.PartitionInitializingAsync -= ProcessInitializingHandler;
                    EventProcessorClient.PartitionClosingAsync -= PartitionClosingHandler;
                    _isInitialized = false;
                }
            }

            _isDisposed = true;
        }
    }
}
