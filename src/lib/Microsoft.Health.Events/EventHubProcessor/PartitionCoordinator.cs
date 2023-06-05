// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Events.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class PartitionCoordinator : IPartitionCoordinator
    {
        private BlobContainerClient _blobContainerClient;

        private ITelemetryLogger _logger;

        private ConcurrentDictionary<string, DateTimeOffset> _ownedPartitions = new ConcurrentDictionary<string, DateTimeOffset>();

        public PartitionCoordinator(BlobContainerClient blobContainerClient, ITelemetryLogger logger)
        {
            _blobContainerClient = blobContainerClient;
            _logger = logger;
        }

        public async Task<bool> ClaimPartitionAsync(string processorId, string partitionId, CancellationToken cancellationToken)
        {
            BlobClient ownershipClient = _blobContainerClient.GetBlobClient($"ownedpartitions/{partitionId}");
            var leaseClient = ownershipClient.GetBlobLeaseClient(processorId);

            await RegisterPartitionBlobIfNotExists(ownershipClient);

            if (_ownedPartitions.ContainsKey(partitionId))
            {
                await leaseClient.RenewAsync();
                return true;
            }
            else
            {
                var result = await leaseClient.AcquireAsync(TimeSpan.FromMinutes(1));
                _ownedPartitions[partitionId] = DateTimeOffset.UtcNow;
                _logger.LogTrace(result.ToString());
                return true;
            }
        }

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

        public async Task<IEnumerable<string>> GetActiveProcessorIdsAsync(CancellationToken cancellationToken)
        {
            var activePods = new List<string>();
            var activeProcessorsInBlob = ListBlobs(_blobContainerClient, "activepods");

            foreach (BlobItem blobItem in activeProcessorsInBlob)
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
                        await _blobContainerClient.DeleteBlobAsync(blobItem.Name);
                    }
                }
            }

            return activePods;
        }

        public async Task<bool> IsPartitionActiveAsync(string partitionId, TimeSpan maxInactiveTime, CancellationToken cancellationToken)
        {
            try
            {
                BlobClient checkPartitionBlobLastModifiedTime = _blobContainerClient.GetBlobClient($"ownedpartitions/{partitionId}");
                var properties = await checkPartitionBlobLastModifiedTime.GetPropertiesAsync();
                var lastModifiedTime = properties.Value.LastModified;

                var delay = DateTimeOffset.UtcNow - lastModifiedTime;
                _logger.LogMetric(EventMetrics.PartitionLastClaimedDelay(partitionId), delay.TotalMilliseconds);

                var delayThreshold = DateTimeOffset.UtcNow - maxInactiveTime;
                if (lastModifiedTime < delayThreshold)
                {
                    _logger.LogError(new UnclaimedPartitionException($"Partition {partitionId} was last claimed at {lastModifiedTime}. The current time is {DateTime.UtcNow}. Delay = {delay.TotalMinutes} minutes"));
                    return false;
                }
                else
                {
                    _logger.LogTrace($"Partition ownership is up to date for partition {partitionId}. The last modified time was {lastModifiedTime}. Delay = {delay.TotalMinutes} minutes");
                    return true;
                }
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                _logger.LogError(new UnclaimedPartitionException($"Partition {partitionId} has never been claimed"));
                _logger.LogMetric(EventMetrics.PartitionLastClaimedDelay(partitionId), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                return false;
            }
        }

        public async Task ResisterProcessorIdAsync(string processorId, CancellationToken cancellationToken)
        {
            // write process id to storage
            _logger.LogTrace($"Processor Id: {processorId}");
            BlobClient blobClient = _blobContainerClient.GetBlobClient($"activepods/{processorId}");

            var activeUntil = new Dictionary<string, string>()
            {
                { "activeuntil", DateTime.UtcNow.AddMinutes(1).ToString() },
            };

            BlobUploadOptions options = new BlobUploadOptions() { Metadata = activeUntil };
            await blobClient.UploadAsync(BinaryData.FromString(string.Empty), options, cancellationToken);

            _logger.LogTrace($"Registered processor {processorId}");
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

        public ConcurrentDictionary<string, DateTimeOffset> GetOwnedPartitions()
        {
            return _ownedPartitions;
        }

        public void ClearOwnedPartitions()
        {
            _ownedPartitions.Clear();
        }

        public Task<IEnumerable<string>> GetUnclaimedPartitionsAsync(IEnumerable<string> partitionsToCheck, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RenewPartitionOwnershipAsync(string processorId, string partitionId, CancellationToken cancellationToken)
        {
            _logger.LogTrace($"Renewing ownership for partition {partitionId}");

            BlobClient ownershipClient = _blobContainerClient.GetBlobClient($"ownedpartitions/{partitionId}");
            var leaseClient = ownershipClient.GetBlobLeaseClient(processorId);
            await leaseClient.RenewAsync(default, cancellationToken);

            // upload to update last modified timestamp
            BlobUploadOptions blobUploadOptions = new BlobUploadOptions()
            {
                Conditions = new BlobRequestConditions()
                {
                    LeaseId = processorId,
                },
            };

            try
            {
                await ownershipClient.UploadAsync(BinaryData.FromString(string.Empty), blobUploadOptions, cancellationToken);
                return true;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
        }
    }
}
