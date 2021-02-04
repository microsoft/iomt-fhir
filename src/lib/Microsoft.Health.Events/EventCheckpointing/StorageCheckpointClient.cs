// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EnsureThat;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventCheckpointing
{
    public class StorageCheckpointClient : ICheckpointClient
    {
        private ConcurrentDictionary<string, Checkpoint> _checkpoints;
        private ConcurrentDictionary<string, int> _lastCheckpointTracker;
        private int _lastCheckpointMaxCount;
        private BlobContainerClient _storageClient;
        private ITelemetryLogger _log;

        public StorageCheckpointClient(BlobContainerClient containerClient, StorageCheckpointOptions options, ITelemetryLogger log)
        {
            EnsureArg.IsNotNull(options);
            BlobPrefix = options.BlobPrefix;

            _lastCheckpointMaxCount = int.Parse(options.CheckpointBatchCount);
            _checkpoints = new ConcurrentDictionary<string, Checkpoint>();
            _lastCheckpointTracker = new ConcurrentDictionary<string, int>();
            _storageClient = containerClient;
            _log = log;
        }

        public string BlobPrefix { get; }

        public BlobContainerClient GetBlobContainerClient()
        {
            return _storageClient;
        }

        public async Task UpdateCheckpointAsync(Checkpoint checkpoint)
        {
            EnsureArg.IsNotNull(checkpoint);
            EnsureArg.IsNotNullOrWhiteSpace(checkpoint.Id);
            var lastProcessed = EnsureArg.IsNotNullOrWhiteSpace(checkpoint.LastProcessed.DateTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

            var blobName = $"{BlobPrefix}/checkpoint/{checkpoint.Id}";
            var blobClient = _storageClient.GetBlobClient(blobName);

            var metadata = new Dictionary<string, string>()
            {
                { "LastProcessed",  lastProcessed },
            };

            try
            {
                try
                {
                    await blobClient.SetMetadataAsync(metadata);
                }
                catch (RequestFailedException ex) when ((ex.ErrorCode == BlobErrorCode.BlobNotFound) || (ex.ErrorCode == BlobErrorCode.ContainerNotFound))
                {
                    using (var blobContent = new MemoryStream(Array.Empty<byte>()))
                    {
                        await blobClient.UploadAsync(blobContent, metadata: metadata).ConfigureAwait(false);
                    }
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _log.LogError(new Exception($"Unable to update checkpoint. {ex.Message}"));
            }
        }

        public Task<Checkpoint> GetCheckpointForPartitionAsync(string partitionIdentifier)
        {
            var prefix = $"{BlobPrefix}/checkpoint/{partitionIdentifier}";

            Task<Checkpoint> GetCheckpointAsync()
            {
                var checkpoint = new Checkpoint();

                foreach (BlobItem blob in _storageClient.GetBlobs(traits: BlobTraits.Metadata, states: BlobStates.All, prefix: prefix, cancellationToken: CancellationToken.None))
                {
                    var partitionId = blob.Name.Split('/').Last();
                    DateTimeOffset lastEventTimestamp = DateTime.MinValue;

                    if (blob.Metadata.TryGetValue("LastProcessed", out var str))
                    {
                        DateTimeOffset.TryParse(str, null, DateTimeStyles.AssumeUniversal, out lastEventTimestamp);
                    }

                    checkpoint.Prefix = BlobPrefix;
                    checkpoint.Id = partitionId;
                    checkpoint.LastProcessed = lastEventTimestamp;
                }

                return Task.FromResult(checkpoint);
            }

            try
            {
                // todo: consider retries
                return GetCheckpointAsync();
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _log.LogError(new Exception($"Unable to get checkpoint for partition. {ex.Message}"));
                throw;
            }
        }

        public async Task SetCheckpointAsync(IEventMessage eventArgs)
        {
            EnsureArg.IsNotNull(eventArgs);
            EnsureArg.IsNotNullOrWhiteSpace(eventArgs.PartitionId);

            try
            {
                var partitionId = eventArgs.PartitionId;
                var checkpoint = new Checkpoint();
                checkpoint.LastProcessed = eventArgs.EnqueuedTime;
                checkpoint.Id = partitionId;
                checkpoint.Prefix = BlobPrefix;

                _checkpoints[partitionId] = checkpoint;
                var count = _lastCheckpointTracker.AddOrUpdate(partitionId, 1, (key, value) => value + 1);

                if (count >= _lastCheckpointMaxCount)
                {
                    await PublishCheckpointAsync(partitionId);
                    _log.LogMetric(EventMetrics.EventWatermark(partitionId, eventArgs.EnqueuedTime.UtcDateTime), 1);
                    _lastCheckpointTracker[partitionId] = 0;
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _log.LogError(new Exception($"Unable to set checkpoint. {ex.Message}"));
            }
        }

        public async Task PublishCheckpointAsync(string partitionId)
        {
            Checkpoint checkpoint = _checkpoints[partitionId];
            await UpdateCheckpointAsync(checkpoint);
        }
    }
}
