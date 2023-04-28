// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Logging.Telemetry;

// Description
// - First, this class gets the number of total processors and total partitions
// - Next, it will calculate how many partitions should be assigned to each processor
// - Next, it will lock the partition(s) using blob leases
// - Next, it will pass the partition id(s) into the AssignedPartitionProcessor which will run until the number of total processors changes

// todos:
// - detect if a partition has no ownership updates for some time
// - handle cases where the ratio of processors to partitions is not divided evenly
// - detect if acquiring partitions takes too long
// - replace Task.Delay with something more sophisticated

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class PartitionLockingBackgroundService : BackgroundService
    {
        private string _processorId;

        private List<int> _ownedPartitions;

        private BlobContainerClient _containerClient;

        private int _totalProcessorsRunning = 0;

        private string[] _eventHubPartitions;

        private EventBatchingService _eventBatchingService;

        private StorageCheckpointClient _checkpointClient;

        private ITelemetryLogger _logger;

        private PartitionLockingBackgroundServiceOptions _partitionLockingOptions;

        private EventHubClientOptions _eventHubClientOptions;

        public PartitionLockingBackgroundService(
            PartitionLockingBackgroundServiceOptions partitionLockingOptions,
            EventHubClientOptions eventHubClientOptions,
            EventBatchingService eventBatchingService,
            StorageCheckpointClient checkpointClient,
            ITelemetryLogger logger)
        {
            _processorId = Guid.NewGuid().ToString(); // consider using the pod name when running in AKS
            _ownedPartitions = new List<int>();
            _partitionLockingOptions = partitionLockingOptions;
            _eventHubClientOptions = eventHubClientOptions;
            _eventBatchingService = eventBatchingService;
            _checkpointClient = checkpointClient;
            _logger = logger;
        }

        private void CreateContainerClient()
        {
            var tokenCredential = EnsureArg.IsNotNull(_partitionLockingOptions.StorageTokenCredential, nameof(_partitionLockingOptions.StorageTokenCredential));
            _containerClient = new BlobContainerClient(_partitionLockingOptions.BlobContainerUri, tokenCredential.GetCredential());
            _containerClient.CreateIfNotExists();
        }

        private async Task RegisterProcessor()
        {
            // write process id to storage
            _logger.LogTrace($"Processor Id: {_processorId}");
            BlobClient blobClient = _containerClient.GetBlobClient($"activepods/{_processorId}");

            var activeUntil = new Dictionary<string, string>()
            {
                { "activeuntil", DateTime.UtcNow.AddMinutes(1).ToString() },
            };

            BlobUploadOptions options = new BlobUploadOptions() { Metadata = activeUntil };
            await blobClient.UploadAsync(BinaryData.FromString(string.Empty), options);

            _logger.LogTrace($"Registered processor {_processorId}");
        }

        private async Task RegisterProcessorLoop()
        {
            while (true)
            {
                await RegisterProcessor();
                await Task.Delay(15000);
            }
        }

        private async Task<string[]> GetPartitionsIdsForEventHub()
        {
            await using (var consumer = new EventHubConsumerClient(
                _eventHubClientOptions.EventHubConsumerGroup,
                _eventHubClientOptions.EventHubNamespaceFQDN,
                _eventHubClientOptions.EventHubName,
                _eventHubClientOptions.EventHubTokenCredential))
            {
                return await consumer.GetPartitionIdsAsync();
            }
        }

        private async Task AcquirePartitions(CancellationToken ct)
        {
            // get all the processors in active status
            var activeProcessors = ListBlobs(_containerClient, "activepods");
            var activeProcessorCount = await GetActiveProcessors(activeProcessors, _containerClient, ct);
            _totalProcessorsRunning = activeProcessorCount;
            _logger.LogTrace($"Active processor count: {activeProcessorCount}");

            if (activeProcessorCount == 0)
            {
                throw new Exception("active processors is 0");
            }

            // divide the partitions among the active pods
            // todo: handle cases where there is a remainder
            // for example 8 partitions and 6 pods, which would require 4 pods to have 1 partition, and 2 pods to have 2 partitions
            // for now, we will round down

            _logger.LogTrace($"The Event Hub has {_eventHubPartitions.Length} partitions");

            var suggestedPartitionsPerProcessor = Math.Floor((double)_eventHubPartitions.Length / activeProcessorCount);
            _logger.LogTrace($"Suggested partitions per processor: {suggestedPartitionsPerProcessor}");

            // find partitions that are not leased, and lease them
            //
            // todo: detect if this takes too long
            // for example, if we acquire 2 partitions right away but need 2 more and cannot get them,
            // should we renew the 2 that we have, or not? Alternatively we could quit after X minutes
            _ownedPartitions.Clear();

            int partitionIdIndex = 0;
            while (_ownedPartitions.Count < suggestedPartitionsPerProcessor && partitionIdIndex < _eventHubPartitions.Length)
            {
                // attempt to claim a partition
                var partitionId = int.Parse(_eventHubPartitions[partitionIdIndex]);
                _logger.LogTrace($"Attempting to claim partition id: {partitionId}");

                try
                {
                    await ClaimPartitionUsingBlob(partitionId);
                }
                catch (RequestFailedException ex)
                {
                    if (ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
                    {
                        _logger.LogTrace($"Failed to acquire partition {partitionId}");
                        await Task.Delay(1000);
                        partitionIdIndex++;

                        // reset if we have reached the end of the partition list but have not acquired enough partitions
                        if (partitionId + 1 >= _eventHubPartitions.Length && _ownedPartitions.Count != suggestedPartitionsPerProcessor)
                        {
                            _logger.LogTrace("Not enough partitions are available for processing... Waiting");
                            await Task.Delay(5000);
                            partitionIdIndex = 0;
                            continue;
                        }

                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }

                // if not enough available partitions then return and wait
                if (partitionId + 1 >= _eventHubPartitions.Length && _ownedPartitions.Count != suggestedPartitionsPerProcessor)
                {
                    _logger.LogTrace("Not enough partitions are available for processing... Waiting");
                    await Task.Delay(5000);
                    partitionIdIndex = 0;
                    continue;
                }

                partitionIdIndex++;
                await Task.Delay(1000);
            }
        }

        private async Task ClaimPartitionUsingBlob(int partitionToClaim)
        {
            BlobClient ownershipClient = _containerClient.GetBlobClient($"ownedpartitions/{partitionToClaim}");
            var leaseClient = ownershipClient.GetBlobLeaseClient(_processorId);

            await RegisterPartitionBlobIfNotExists(ownershipClient);

            if (_ownedPartitions.Contains(partitionToClaim))
            {
                await leaseClient.RenewAsync();
            }
            else
            {
                var result = await leaseClient.AcquireAsync(TimeSpan.FromMinutes(1));
                _ownedPartitions.Add(partitionToClaim);
                _logger.LogTrace(result.ToString());
            }
        }

        // registers a partition as a blob so that it can be leased later by a processor
        private async Task RegisterPartitionBlobIfNotExists(BlobClient ownershipClient)
        {
            if (await ownershipClient.ExistsAsync())
            {
                return;
            }
            else
            {
                var eTag = default(ETag);
                BlobUploadOptions ownershipClientOptions = new BlobUploadOptions()
                {
                    Conditions = new BlobRequestConditions()
                    {
                        IfMatch = eTag,
                    },
                };

                try
                {
                    await ownershipClient.UploadAsync(BinaryData.FromString(string.Empty), ownershipClientOptions);
                }
                catch (RequestFailedException ex)
                {
                    if (ex.ErrorCode == BlobErrorCode.ConditionNotMet)
                    {
                        _logger.LogTrace($"Another processor uploaded a blob for partition id {ownershipClient.Name}");
                        return;
                    }

                    throw;
                }
            }
        }

        private static Pageable<BlobItem> ListBlobs(BlobContainerClient blobContainerClient, string prefix)
        {
            try
            {
                var resultSegment = blobContainerClient.GetBlobs(BlobTraits.Metadata, BlobStates.None, prefix);
                return resultSegment;
            }
            catch
            {
                throw;
            }
        }

        private async Task<int> GetActiveProcessors(Pageable<BlobItem> resultSegment, BlobContainerClient containerClient, CancellationToken ct)
        {
            var activePods = new HashSet<string>();

            foreach (BlobItem blobItem in resultSegment)
            {
                // if the processor has been updated in the last minute then consider it active
                // otherwise delete it
                if (blobItem.Metadata.TryGetValue("activeuntil", out var activeUntil))
                {
                    if (DateTime.Parse(activeUntil) > DateTime.UtcNow.AddMinutes(-1))
                    {
                        _logger.LogTrace($"Blob name: {blobItem.Name} is active");
                        activePods.Add(blobItem.Name);
                    }
                    else
                    {
                        // todo: check the etag as a condition for delete
                        _logger.LogTrace($"Deleting blob {blobItem.Name}. It was last active {activeUntil}.");
                        await containerClient.DeleteBlobAsync(blobItem.Name);
                    }
                }
            }

            return activePods.Count;
        }

        public async Task RunAndRenew(CancellationTokenSource cts)
        {
            // in the background we will continually update partition ownership until
            // we receive a cancellation token to shutdown the processor
            _logger.LogTrace($"Running the processor with partitions: {string.Join(", ", _ownedPartitions.ToArray())}");

            try
            {
                await RenewOwnership(cts.Token);
                await CheckProcessorCount(cts);
                await StartAssignedPartitionProcessor(cts.Token, _ownedPartitions);
            }
            catch (TaskCanceledException)
            {
                _logger.LogTrace("A task was cancelled");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            CreateContainerClient();
            await RegisterProcessor();
            await ContinuallyRegisterProcessor();

            while (true)
            {
                using var innerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                while (!innerCts.IsCancellationRequested)
                {
                    await Task.Delay(5000);
                    _eventHubPartitions = await GetPartitionsIdsForEventHub();
                    await AcquirePartitions(innerCts.Token);
                    await RunAndRenew(innerCts);
                }
            }
        }

        private Task ContinuallyRegisterProcessor()
        {
            Task.Run(() => RegisterProcessorLoop());
            return Task.CompletedTask;
        }

        private Task CheckProcessorCount(CancellationTokenSource cts)
        {
            Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    var activePods = ListBlobs(_containerClient, "activepods");
                    var activeProcessors = await GetActiveProcessors(activePods, _containerClient, cts.Token);

                    _logger.LogTrace($"CheckProcessorCount - active processors {activeProcessors}");

                    if (activeProcessors != _totalProcessorsRunning)
                    {
                        _logger.LogTrace($"Detected that number of processors equals {activeProcessors} but this processor is running with {_totalProcessorsRunning} known processors");
                        _logger.LogTrace("Restarting and recomputing");
                        cts.Cancel();
                    }

                    await Task.Delay(30000, cts.Token);
                }
            });

            return Task.CompletedTask;
        }

        private Task RenewOwnership(CancellationToken ct)
        {
            Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    foreach (var partition in _ownedPartitions)
                    {
                        _logger.LogTrace($"Renewing ownership for partition {partition}");
                        BlobClient ownershipClient = _containerClient.GetBlobClient($"ownedpartitions/{partition}");
                        var leaseClient = ownershipClient.GetBlobLeaseClient(_processorId);
                        await leaseClient.RenewAsync(default, ct);
                    }

                    await Task.Delay(20000);
                }
            });

            return Task.CompletedTask;
        }

        private async Task StartAssignedPartitionProcessor(CancellationToken ct, List<int> ownedPartitions)
        {
            var maximumBatchSize = 100;

            string[] partitions = new string[ownedPartitions.Count];
            for (var i = 0; i < ownedPartitions.Count; i++)
            {
                partitions[i] = ownedPartitions.ElementAt(i).ToString();
            }

            var eventHubName = EnsureArg.IsNotNull(_eventHubClientOptions.EventHubName, nameof(_eventHubClientOptions.EventHubName));
            var eventHubNamespaceFQDN = EnsureArg.IsNotNull(_eventHubClientOptions.EventHubNamespaceFQDN, nameof(_eventHubClientOptions.EventHubNamespaceFQDN));
            var eventHubConsumerGroup = EnsureArg.IsNotNull(_eventHubClientOptions.EventHubConsumerGroup, nameof(_eventHubClientOptions.EventHubConsumerGroup));
            var tokenCrendential = EnsureArg.IsNotNull(_eventHubClientOptions.EventHubTokenCredential, nameof(_eventHubClientOptions.EventHubTokenCredential));

            var processor = new AssignedPartitionProcessor(
                _eventBatchingService,
                _checkpointClient,
                _logger,
                partitions,
                maximumBatchSize,
                eventHubConsumerGroup,
                tokenCrendential,
                eventHubName,
                eventHubNamespaceFQDN);

            try
            {
                await processor.StartProcessingAsync(ct);
                await Task.Delay(Timeout.Infinite, ct);
            }
            catch (TaskCanceledException)
            {
                // This is expected if the cancellation token is signaled.
                _logger.LogTrace("AssignedPartitionProcessor has received a cancellation request");
            }
            finally
            {
                // Stopping may take up to the length of time defined
                // as the TryTimeout configured for the processor;
                // By default, this is 60 seconds.

                await processor.StopProcessingAsync();
            }
        }
    }
}
