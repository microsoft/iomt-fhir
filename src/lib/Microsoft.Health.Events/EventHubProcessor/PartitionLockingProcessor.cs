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
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

// description
// - Get the number of total processors and total partitions
// - Calculate how many partitions should be assigned to each processor
// - Lock the partition(s)
// - Pass the partition ids into the StaticPartitionProcessor and run until the number of total processors changes

// usage
//  var cts = new CancellationTokenSource();
//  var processor = new PartitionLockingProcessor();
//  await processor.RunProcessor(cts);

// todos:
// - detect if a partition has no ownership updates for some time
// - handle cases where the ratio of processors to partitions is not divided evenly
// - detect if acquiring partitions takes too long
// - replace Task.Delay with something more sophisticated

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class PartitionLockingProcessor
    {
        private string _processorId;

        private List<int> _ownedPartitions;

        private BlobContainerClient _containerClient;

        private int _totalProcessorsRunning = 0;

        // consider injecting these or querying event hub for max partitions
        private int _maximumProcessors = 8;

        private int _maximumPartitions = 8;

        // settings that should be passed in as environment variables
        private Uri _paritionManagerUri = new Uri("<blob uri goes here>");

        private Uri _storageCheckpointUri = new Uri("<blob uri goes here>");

        private string _eventHubConnectionString = "<connection string>";

        private string _eventHubName = "<event hub name>";

        private string _eventHubConsumerGroup = "$Default";

        private TokenCredential _tokenCredential;

        public PartitionLockingProcessor()
        {
            _processorId = Guid.NewGuid().ToString(); // consider using the pod name when running in AKS
            _ownedPartitions = new List<int>();
            _tokenCredential = new DefaultAzureCredential();
        }

        private void CreateContainerClient()
        {
            _containerClient = new BlobContainerClient(_paritionManagerUri, _tokenCredential);
            _containerClient.CreateIfNotExists();
        }

        private async Task RegisterProcessor()
        {
            // write process id to storage
            Console.WriteLine($"Processor Id: {_processorId}");
            BlobClient blobClient = _containerClient.GetBlobClient($"activepods/{_processorId}");

            var activeUntil = new Dictionary<string, string>()
            {
                { "activeuntil", DateTime.UtcNow.AddMinutes(1).ToString() },
            };

            BlobUploadOptions options = new BlobUploadOptions() { Metadata = activeUntil };
            await blobClient.UploadAsync(BinaryData.FromString(string.Empty), options);

            Console.WriteLine($"Registered processor {_processorId}");
        }

        private async Task RegisterProcessorLoop()
        {
            while (true)
            {
                await RegisterProcessor();
                await Task.Delay(15000);
            }
        }

        private async Task AcquirePartitions(CancellationToken ct)
        {
            // get all the pods in active status
            var activePods = ListBlobs(_containerClient, "activepods");
            var activePodCount = await GetActiveProcessors(activePods, _containerClient);
            _totalProcessorsRunning = activePodCount;
            Console.WriteLine($"Active Pod Count: {activePodCount}");

            if (activePodCount == 0)
            {
                throw new Exception("active pods is 0");
            }

            // divide the partitions among the pods
            // todo: handle cases where there is a remainder
            // for example 8 partitions and 6 pods, which would require 4 pods to have 1 partition, and 2 pods to have 2 partitions
            // for now, we will round down
            var suggestedPartitionsPerPod = Math.Floor((double)_maximumProcessors / activePodCount);
            Console.WriteLine($"Suggested Partitions Per Pod: {suggestedPartitionsPerPod}");

            // find partitions without owners
            // then aquire partitions one by one using ETags
            //
            // todo: detect if this takes too long
            // for example, if we acquire 2 partitions right away but need 2 more and cannot get them,
            // should we renew the 2 that we have, or not? Alternatively we could quit after X minutes
            _ownedPartitions.Clear();

            while (_ownedPartitions.Count < suggestedPartitionsPerPod)
            {
                HashSet<int> availablePartitions = new HashSet<int>();
                for (int i = 0; i < _maximumPartitions; i++)
                {
                    availablePartitions.Add(i);
                }

                Dictionary<int, BlobItem> blobOwnerhip = new Dictionary<int, BlobItem>();

                // get owned partitions
                var ownershipList = ListBlobs(_containerClient, "ownedpartitions");

                // remove owned partitions from set of available partitions
                foreach (var ownership in ownershipList)
                {
                    ownership.Metadata.TryGetValue("owner", out var owner);
                    ownership.Metadata.TryGetValue("owneduntil", out var ownedUntil);

                    int partitionId = int.Parse(ownership.Name.Split('/')[1]);

                    // if the partition is still owned then remove it from the list of available partitions
                    if (DateTime.Parse(ownedUntil) > DateTime.UtcNow)
                    {
                        availablePartitions.Remove(partitionId);
                    }

                    blobOwnerhip.Add(partitionId, ownership);
                }

                // if not available partitions then return and wait
                if (availablePartitions.Count == 0)
                {
                    Console.WriteLine("No partitions are available... Waiting");
                    await Task.Delay(5000);
                    continue;
                }

                // pick a random available partition
                var random = new Random();
                var partitionToClaim = availablePartitions.ElementAt(random.Next(availablePartitions.Count));
                Console.WriteLine($"Attempting to claim partition id: {partitionToClaim}");

                BlobClient ownershipClient = _containerClient.GetBlobClient($"ownedpartitions/{partitionToClaim}");

                ETag eTag;
                if (blobOwnerhip.ContainsKey(partitionToClaim))
                {
                    eTag = (ETag)blobOwnerhip[partitionToClaim].Properties.ETag;
                }
                else
                {
                    eTag = default(ETag);
                }

                BlobUploadOptions ownershipClientOptions = new BlobUploadOptions()
                {
                    Metadata = new Dictionary<string, string>()
                    {
                        { "owner", _processorId },
                        { "owneduntil", DateTime.UtcNow.AddMinutes(1).ToString() },
                    },
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
                        Console.WriteLine($"Failed to acquire partition {partitionToClaim} due to ETag mismatch");
                        await Task.Delay(1000);
                        continue;
                    }

                    throw;
                }

                _ownedPartitions.Add(partitionToClaim);
                await Task.Delay(1000);
            }
        }

        private static Pageable<BlobItem> ListBlobs(BlobContainerClient blobContainerClient, string prefix)
        {
            try
            {
                var resultSegment = blobContainerClient.GetBlobs(BlobTraits.Metadata, BlobStates.None, prefix);
                return resultSegment;
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        private static async Task<int> GetActiveProcessors(Pageable<BlobItem> resultSegment, BlobContainerClient containerClient)
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
                        Console.WriteLine("Blob name: {0} is active", blobItem.Name);
                        activePods.Add(blobItem.Name);
                    }
                    else
                    {
                        // todo: check the etag as a condition for delete
                        Console.WriteLine($"Deleting blob {blobItem.Name}. It was last active {activeUntil}.");
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

            Console.WriteLine("Running the processor with partitions: ");
            foreach (var p in _ownedPartitions)
            {
                Console.WriteLine(p);
            }

            try
            {
                await RenewOwnership(cts.Token);
                await CheckProcessorCount(cts);
                await StartStaticPartitionProcessor(cts.Token, _ownedPartitions);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("A task was cancelled");
            }
        }

        public async Task RunProcessor(CancellationTokenSource cts)
        {
            CreateContainerClient();
            await RegisterProcessor();
            await ContinuallyRegisterProcessor();

            while (true)
            {
                var innerCts = new CancellationTokenSource();
                while (!innerCts.IsCancellationRequested)
                {
                    await Task.Delay(5000);
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
                    var activeProcessors = await GetActiveProcessors(activePods, _containerClient);

                    Console.WriteLine($"CheckProcessorCount - active processors {activeProcessors}");

                    if (activeProcessors != _totalProcessorsRunning)
                    {
                        Console.WriteLine($"Detected that number of processors equals {activeProcessors} but this processor is running with {_totalProcessorsRunning} known processors");
                        Console.WriteLine("Restarting and recomputing");
                        cts.Cancel();
                    }

                    await Task.Delay(30000);
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
                        Console.WriteLine($"Renewing ownership for partition {partition}");

                        // get the blob and ETag
                        BlobClient ownershipClient = _containerClient.GetBlobClient($"ownedpartitions/{partition}");
                        var properties = await ownershipClient.GetPropertiesAsync();
                        var eTag = properties.Value.ETag;

                        BlobUploadOptions ownershipClientOptions = new BlobUploadOptions()
                        {
                            Metadata = new Dictionary<string, string>()
                            {
                                { "owner", _processorId },
                                { "owneduntil", DateTime.UtcNow.AddMinutes(1).ToString() },
                            },
                            Conditions = new BlobRequestConditions()
                            {
                                IfMatch = eTag,
                            },
                        };

                        var ownershipResult = await ownershipClient.UploadAsync(BinaryData.FromString(string.Empty), ownershipClientOptions);

                        // if 412 Precondition Failed is returned then that is weird
                        if (ownershipResult.GetRawResponse().Status == 412)
                        {
                            throw new Exception("HTTP 412 Precondition failed when renewing partition ownership");
                        }
                    }

                    await Task.Delay(15000);
                }
            });

            return Task.CompletedTask;
        }

        private async Task StartStaticPartitionProcessor(CancellationToken ct, List<int> ownedPartitions)
        {
            var storageClient = new BlobContainerClient(_storageCheckpointUri, _tokenCredential);

            var maximumBatchSize = 100;

            string[] partitions = new string[ownedPartitions.Count];
            for (var i = 0; i < ownedPartitions.Count; i++)
            {
                partitions[i] = ownedPartitions.ElementAt(i).ToString();
            }

            var processor = new StaticPartitionProcessor(
                storageClient,
                partitions,
                maximumBatchSize,
                _eventHubConsumerGroup,
                _eventHubConnectionString,
                _eventHubName);

            try
            {
                await processor.StartProcessingAsync(ct);
                await Task.Delay(Timeout.Infinite, ct);
            }
            catch (TaskCanceledException)
            {
                // This is expected if the cancellation token is signaled.
                Console.WriteLine("StartStaticPartitionProcessor has received a cancellation request");
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
