// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs.Models;
using EnsureThat;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Logging.Telemetry;

/*
 * Description
   - First, this class gets the number of total processors and total partitions
   - Next, it will calculate how many partitions should be assigned to each processor
   - Next, it will lock the partition(s) using blob leases
   - Next, it will pass the partition id(s) into the AssignedPartitionProcessor which will run until the number of total processors changes

 * For a flowchart of the PartitionLockingService see: \iomt-fhir\docs\PartitionLocking.md

 * TODO:
   - Handle cases where the ratio of processors to partitions is not divided evenly
   - Detect if acquiring partitions takes too long
*/

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class PartitionLockingService
    {
        private string _processorId;

        private List<string> _ownedPartitions;

        private int _totalProcessorsRunning = 0;

        private string[] _eventHubPartitions;

        private EventBatchingService _eventBatchingService;

        private EventBatchingOptions _eventBatchingOptions;

        private ICheckpointClient _checkpointClient;

        private ITelemetryLogger _logger;

        private EventHubClientOptions _eventHubClientOptions;

        private EventHubConsumerClient _eventHubPartitionCountClient;

        private IAssignedPartitionProcessorFactory _assignedPartitionProcessorFactory;

        private IPartitionCoordinator _partitionCoordinator;

        public PartitionLockingService(
            IProcessorIdProvider processorIdProvider,
            EventHubConsumerClient eventHubPartitionCountClient,
            EventHubClientOptions eventHubClientOptions,
            EventBatchingService eventBatchingService,
            EventBatchingOptions eventBatchingOptions,
            ICheckpointClient checkpointClient,
            IPartitionCoordinator partitionCoordinator,
            IAssignedPartitionProcessorFactory processorFactory,
            ITelemetryLogger logger)
        {
            _processorId = processorIdProvider.GetProcessorId();
            _ownedPartitions = new List<string>();
            _partitionCoordinator = partitionCoordinator;
            _eventHubClientOptions = eventHubClientOptions;
            _eventBatchingService = eventBatchingService;
            _eventBatchingOptions = eventBatchingOptions;
            _assignedPartitionProcessorFactory = processorFactory;
            _checkpointClient = checkpointClient;
            _logger = logger;

            _eventHubPartitionCountClient = eventHubPartitionCountClient; // used to check if number of event hub partitions has changed
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await _partitionCoordinator.ResisterProcessorIdAsync(_processorId, ct);
            await RegisterProcessorAsBackgroundTask(ct);
            await CheckIfPartitionsAreUnclaimed(ct);

            while (!ct.IsCancellationRequested)
            {
                using var innerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                while (!innerCts.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), innerCts.Token);
                    _eventHubPartitions = await GetPartitionsIdsForEventHub(innerCts.Token);
                    await AcquirePartitions(innerCts.Token);
                    await RunProcessorAndRenewPartitionOwnership(innerCts);
                }
            }
        }

        private async Task RunProcessorAndRenewPartitionOwnership(CancellationTokenSource cts)
        {
            // in the background we will continually update partition ownership until
            // we receive a cancellation token to shutdown the processor
            _logger.LogTrace($"Running the processor with partitions: {string.Join(", ", _partitionCoordinator.GetOwnedPartitions().Select(i => i.Key.ToString()).ToArray())}");

            try
            {
                await RenewOwnershipAsBackgroundTask(cts.Token);
                await CheckProcessorCountAsBackgroundTask(cts);
                await StartAssignedPartitionProcessor(cts.Token, _partitionCoordinator.GetOwnedPartitions());
            }
            catch (TaskCanceledException)
            {
                _logger.LogTrace("A task was cancelled");
            }
        }

        private async Task RegisterProcessorLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await _partitionCoordinator.ResisterProcessorIdAsync(_processorId, ct);
                await Task.Delay(TimeSpan.FromSeconds(15), ct);
            }
        }

        private async Task<string[]> GetPartitionsIdsForEventHub(CancellationToken ct)
        {
            return await _eventHubPartitionCountClient.GetPartitionIdsAsync(ct);
        }

        private async Task AcquirePartitions(CancellationToken ct)
        {
            // get all the processors in active status
            var activeProcessors = await _partitionCoordinator.GetActiveProcessorIdsAsync(ct);
            var activeProcessorCount = activeProcessors.Count();
            _totalProcessorsRunning = activeProcessorCount;
            _logger.LogTrace($"Active processor count: {activeProcessorCount}");

            if (activeProcessorCount == 0)
            {
                throw new ProcessorCountException("The number of active processors is 0");
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
            _partitionCoordinator.ClearOwnedPartitions();

            int partitionIdIndex = 0;

            while (_partitionCoordinator.GetOwnedPartitions().Count < suggestedPartitionsPerProcessor && partitionIdIndex < _eventHubPartitions.Length)
            {
                // attempt to claim a partition
                var partitionId = _eventHubPartitions[partitionIdIndex];
                _logger.LogTrace($"Attempting to claim partition id: {partitionId}");

                try
                {
                    await _partitionCoordinator.ClaimPartitionAsync(_processorId, partitionId, ct);
                }
                catch (RequestFailedException ex)
                {
                    if (ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
                    {
                        _logger.LogTrace($"Failed to acquire partition {partitionId}");
                        await Task.Delay(TimeSpan.FromSeconds(1), ct);
                        partitionIdIndex++;

                        // reset if we have reached the end of the partition list but have not acquired enough partitions
                        if (int.Parse(partitionId) + 1 >= _eventHubPartitions.Length && _partitionCoordinator.GetOwnedPartitions().Count != suggestedPartitionsPerProcessor)
                        {
                            _logger.LogTrace($"Not enough partitions are available for processing... Suggested {suggestedPartitionsPerProcessor}. Currently own {_ownedPartitions.Count}. Waiting");
                            await Task.Delay(TimeSpan.FromSeconds(5), ct);
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
                if (int.Parse(partitionId) + 1 >= _eventHubPartitions.Length && _partitionCoordinator.GetOwnedPartitions().Count != suggestedPartitionsPerProcessor)
                {
                    _logger.LogTrace("Not enough partitions are available for processing... Waiting");
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
                    partitionIdIndex = 0;
                    continue;
                }

                partitionIdIndex++;
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }
        }

        private Task RegisterProcessorAsBackgroundTask(CancellationToken ct)
        {
            Task.Run(() => RegisterProcessorLoop(ct), ct);
            return Task.CompletedTask;
        }

        private Task CheckProcessorCountAsBackgroundTask(CancellationTokenSource cts)
        {
            Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    var activeProcessors = await _partitionCoordinator.GetActiveProcessorIdsAsync(cts.Token);
                    var activeProcessorCount = activeProcessors.Count();

                    _logger.LogTrace($"CheckProcessorCount - active processors {activeProcessorCount}");

                    if (activeProcessorCount != _totalProcessorsRunning)
                    {
                        _logger.LogTrace($"Detected that number of processors equals {activeProcessorCount} but this processor is running with {_totalProcessorsRunning} known processors");
                        _logger.LogTrace("Restarting and recomputing");

                        // Cancel the main loop and restart so that the application can recompute the partitions it needs to be assigned
                        cts.Cancel();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30), cts.Token);
                }
            });

            return Task.CompletedTask;
        }

        private Task CheckIfPartitionsAreUnclaimed(CancellationToken ct)
        {
            Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), ct);
                    var maximumDelayThreshold = TimeSpan.FromMinutes(5);

                    _logger.LogTrace("Checking if any partitions are unclaimed");

                    foreach (var partition in _eventHubPartitions)
                    {
                        await _partitionCoordinator.IsPartitionActiveAsync(partition, maximumDelayThreshold, ct);
                    }
                }
            });

            return Task.CompletedTask;
        }

        private Task RenewOwnershipAsBackgroundTask(CancellationToken ct)
        {
            Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    foreach (var partition in _ownedPartitions)
                    {
                        await _partitionCoordinator.RenewPartitionOwnershipAsync(_processorId, partition, ct);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(20), ct);
                }
            });

            return Task.CompletedTask;
        }

        private async Task StartAssignedPartitionProcessor(CancellationToken ct, ConcurrentDictionary<string, DateTimeOffset> ownedPartitions)
        {
            string[] partitions = ownedPartitions.Select(i => i.Key.ToString()).ToArray();

            var eventHubName = EnsureArg.IsNotNull(_eventHubClientOptions.EventHubName, nameof(_eventHubClientOptions.EventHubName));
            var eventHubNamespaceFQDN = EnsureArg.IsNotNull(_eventHubClientOptions.EventHubNamespaceFQDN, nameof(_eventHubClientOptions.EventHubNamespaceFQDN));
            var eventHubConsumerGroup = EnsureArg.IsNotNull(_eventHubClientOptions.EventHubConsumerGroup, nameof(_eventHubClientOptions.EventHubConsumerGroup));
            var tokenCrendential = EnsureArg.IsNotNull(_eventHubClientOptions.EventHubTokenCredential, nameof(_eventHubClientOptions.EventHubTokenCredential));

            var assignedPartitionProcessor = _assignedPartitionProcessorFactory.CreateAssignedPartitionProcessor(
                _eventBatchingService,
                _checkpointClient,
                _logger,
                partitions,
                _eventBatchingOptions.MaxEvents,
                eventHubConsumerGroup,
                tokenCrendential,
                eventHubName,
                eventHubNamespaceFQDN,
                default);

            try
            {
                await assignedPartitionProcessor.StartProcessingAsync(ct);
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

                await assignedPartitionProcessor.StopProcessingAsync();
                _logger.LogTrace("AssignedPartitionProcessor has finished StopProcessingAsync()");
            }
        }
    }
}
