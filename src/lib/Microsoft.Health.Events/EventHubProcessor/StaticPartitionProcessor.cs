// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class StaticPartitionProcessor : PluggableCheckpointStoreEventProcessor<EventProcessorPartition>
    {
        // This example uses a connection string, so only the single constructor
        // was implemented; applications will need to shadow each constructor of
        // the PluggableCheckpointStoreEventProcessor that they are using.

        private readonly string[] _assignedPartitions;

        public StaticPartitionProcessor(
            BlobContainerClient storageClient,
            string[] assignedPartitions,
            int eventBatchMaximumCount,
            string consumerGroup,
            string connectionString,
            string eventHubName,
            EventProcessorOptions clientOptions = default)
                : base(
                    new BlobCheckpointStore(storageClient),
                    eventBatchMaximumCount,
                    consumerGroup,
                    connectionString,
                    eventHubName,
                    clientOptions)
        {
            _assignedPartitions = assignedPartitions
                ?? throw new ArgumentNullException(nameof(assignedPartitions));
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected async override Task OnProcessingEventBatchAsync(
            IEnumerable<EventData> events,
            EventProcessorPartition partition,
            CancellationToken cancellationToken)
        {
            EventData lastEvent = null;

            try
            {
                Console.WriteLine($"Received events for partition {partition.PartitionId}");

                foreach (var currentEvent in events)
                {
                    // Console.WriteLine($"Event: {currentEvent.EventBody}");
                    lastEvent = currentEvent;
                }

                if (lastEvent != null)
                {
                    // await UpdateCheckpointAsync(
                    //    partition.PartitionId,
                    //    lastEvent.Offset,
                    //    lastEvent.SequenceNumber,
                    //    cancellationToken)
                    // .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // It is very important that you always guard against exceptions in
                // your handler code; the processor does not have enough
                // understanding of your code to determine the correct action to take.
                // Any exceptions from your handlers go uncaught by the processor and
                // will NOT be redirected to the error handler.
                //
                // In this case, the partition processing task will fault and be restarted
                // from the last recorded checkpoint.

                Console.WriteLine($"Exception while processing events: {ex}");
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        protected override Task OnProcessingErrorAsync(
            Exception exception,
            EventProcessorPartition partition,
            string operationDescription,
            CancellationToken cancellationToken)
        {
            try
            {
                if (partition != null)
                {
                    Console.Error.WriteLine(
                        $"Exception on partition {partition.PartitionId} while " +
                        $"performing {operationDescription}: {exception}");
                }
                else
                {
                    Console.Error.WriteLine(
                        $"Exception while performing {operationDescription}: {exception}");
                }
            }
            catch (Exception ex)
            {
                // It is very important that you always guard against exceptions
                // in your handler code; the processor does not have enough
                // understanding of your code to determine the correct action to
                // take.  Any exceptions from your handlers go uncaught by the
                // processor and will NOT be handled in any way.
                //
                // In this case, unhandled exceptions will not impact the processor
                // operation but will go unobserved, hiding potential application problems.

                Console.WriteLine($"Exception while processing events: {ex}");
            }

            return Task.CompletedTask;
        }

        protected override Task OnInitializingPartitionAsync(
            EventProcessorPartition partition,
            CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"Initializing partition {partition.PartitionId}");
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

                Console.WriteLine($"Exception while initializing a partition: {ex}");
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
                Console.WriteLine(
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

                Console.WriteLine($"Exception while stopping processing for a partition: {ex}");
            }

            return Task.CompletedTask;
        }
    }
}
