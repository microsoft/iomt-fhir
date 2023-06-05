// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Primitives;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventHubProcessor
{
    /// <summary>
    /// This class was modeled after a sample Event Hub custom processor class
    /// </summary>
    /// <see href="Sample08_CustomEventProcessor">https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs/samples/Sample08_CustomEventProcessor.md</see>
    public class AssignedPartitionProcessor : PluggableCheckpointStoreEventProcessor<EventProcessorPartition>
    {
        private IEventConsumerService _eventConsumerService;

        private ICheckpointClient _checkpointClient;

        private ITelemetryLogger _logger;

        private readonly string[] _assignedPartitions;

        public AssignedPartitionProcessor(
            IEventConsumerService eventConsumerService,
            ICheckpointClient checkpointClient,
            ITelemetryLogger logger,
            string[] assignedPartitions,
            int eventBatchMaximumCount,
            string consumerGroup,
            TokenCredential tokenCredential,
            string eventHubName,
            string fullyQualifiedNamespace,
            EventProcessorOptions clientOptions = default)
                : base(
                    new BlobCheckpointStore(GetBlobClient(checkpointClient)),
                    eventBatchMaximumCount,
                    consumerGroup,
                    fullyQualifiedNamespace,
                    eventHubName,
                    tokenCredential,
                    clientOptions)
        {
            _logger = logger;
            _eventConsumerService = eventConsumerService;
            _checkpointClient = checkpointClient;
            _assignedPartitions = assignedPartitions
                ?? throw new ArgumentNullException(nameof(assignedPartitions));
        }

        private static BlobContainerClient GetBlobClient(ICheckpointClient checkpointClient)
        {
            var client = (StorageCheckpointClient)checkpointClient;
            return client.GetBlobContainerClient();
        }

        // To simplify logic, tell the processor that only its assigned
        // partitions exist for the Event Hub.

        protected override Task<string[]> ListPartitionIdsAsync(
            EventHubConnection connection,
            CancellationToken cancellationToken) =>
                Task.FromResult(_assignedPartitions);

        // Tell the processor that it owns all of the available partitions for the Event Hub.
        protected override Task<IEnumerable<EventProcessorPartitionOwnership>> ListOwnershipAsync(
            CancellationToken cancellationToken) =>
                Task.FromResult(
                    _assignedPartitions.Select(partition =>
                        new EventProcessorPartitionOwnership
                        {
                            FullyQualifiedNamespace = FullyQualifiedNamespace,
                            EventHubName = EventHubName,
                            ConsumerGroup = ConsumerGroup,
                            PartitionId = partition.ToString(),
                            OwnerIdentifier = Identifier,
                            LastModifiedTime = DateTimeOffset.UtcNow,
                        }));

        // Accept any ownership claims attempted by the processor; this allows the processor to
        // simulate renewing ownership so that it continues to own all of its assigned partitions.
        protected override Task<IEnumerable<EventProcessorPartitionOwnership>> ClaimOwnershipAsync(
           IEnumerable<EventProcessorPartitionOwnership> desiredOwnership,
           CancellationToken cancellationToken) =>
                Task.FromResult(desiredOwnership.Select(ownership =>
                {
                    ownership.LastModifiedTime = DateTimeOffset.UtcNow;
                    return ownership;
                }));

        protected async override Task OnProcessingEventBatchAsync(
            IEnumerable<EventData> events,
            EventProcessorPartition partition,
            CancellationToken cancellationToken)
        {
            try
            {
                bool isEventBatchEmpty = true;

                foreach (var currentEvent in events)
                {
                    isEventBatchEmpty = false;

                    var eventMessage = new EventMessage(
                        partition.PartitionId,
                        currentEvent.EventBody,
                        currentEvent.ContentType,
                        currentEvent.SequenceNumber,
                        currentEvent.Offset,
                        currentEvent.EnqueuedTime,
                        currentEvent.Properties,
                        currentEvent.SystemProperties);

                    await _eventConsumerService.ConsumeEvent(eventMessage, cancellationToken);
                }

                if (isEventBatchEmpty)
                {
                    var maxWaitEvent = new MaximumWaitEvent(partition.PartitionId, DateTime.UtcNow);
                    await _eventConsumerService.ConsumeEvent(maxWaitEvent, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"OnProcessingEventBatchAsync failed to process event batch {ex.Message}");
                _logger.LogError(ex);
            }
        }

        protected override Task OnProcessingErrorAsync(
            Exception exception,
            EventProcessorPartition partition,
            string operationDescription,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            return Task.CompletedTask;
        }

        protected async override Task<EventProcessorCheckpoint> GetCheckpointAsync(
            string partitionId,
            CancellationToken cancellationToken)
        {
            var checkpointValue = await _checkpointClient.GetCheckpointForPartitionAsync(partitionId, cancellationToken);

            if (checkpointValue != null)
            {
                _logger.LogTrace($"Starting to process events from {checkpointValue.LastProcessed} on partition {partitionId}");

                return new EventProcessorCheckpoint()
                {
                    FullyQualifiedNamespace = FullyQualifiedNamespace,
                    EventHubName = EventHubName,
                    ConsumerGroup = ConsumerGroup,
                    PartitionId = partitionId,
                    StartingPosition = EventPosition.FromEnqueuedTime(checkpointValue.LastProcessed),
                };
            }
            else
            {
                _logger.LogTrace($"No checkpoint found for partition {partitionId}");
                return null;
            }
        }

        protected override Task OnInitializingPartitionAsync(
            EventProcessorPartition partition,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogTrace($"Initializing partition {partition.PartitionId}");
            }
            catch (Exception ex)
            {
                // It is very important that you always guard against exceptions in
                // your handler code; the processor does not have enough
                // understanding of your code to determine the correct action to take.
                // Any exceptions from your handlers go uncaught by the processor and
                // will NOT be redirected to the error handler.
                //
                // In this case, the partition processing task will fault and the
                // partition will be initialized again.

                _logger.LogError(ex);
            }

            return Task.CompletedTask;
        }

        protected override Task OnPartitionProcessingStoppedAsync(
            EventProcessorPartition partition,
            ProcessingStoppedReason reason,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogTrace(
                    $"No longer processing partition {partition.PartitionId} " +
                    $"because {reason}");
            }
            catch (Exception ex)
            {
                // It is very important that you always guard against exceptions in
                // your handler code; the processor does not have enough
                // understanding of your code to determine the correct action to take.
                // Any exceptions from your handlers go uncaught by the processor and
                // will NOT be redirected to the error handler.
                //
                // In this case, unhandled exceptions will not impact the processor
                // operation but will go unobserved, hiding potential application problems.

                _logger.LogError(ex);
            }

            return Task.CompletedTask;
        }
    }
}
