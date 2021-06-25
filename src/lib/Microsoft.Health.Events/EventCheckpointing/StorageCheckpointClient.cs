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
using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EnsureThat;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventCheckpointing
{
    public class StorageCheckpointClient : ICheckpointClient
    {
        private readonly string _blobCheckpointPrefix;
        private readonly string _blobPath;
        private readonly ConcurrentDictionary<string, Checkpoint> _checkpoints;
        private readonly int _lastCheckpointMaxCount;
        private readonly ConcurrentDictionary<string, int> _lastCheckpointTracker;
        private readonly ITelemetryLogger _log;
        private readonly BlobContainerClient _storageClient;

        public StorageCheckpointClient(BlobContainerClient containerClient, StorageCheckpointOptions storageCheckpointOptions, EventHubClientOptions eventHubClientOptions, ITelemetryLogger log)
        {
            EnsureArg.IsNotNull(containerClient, nameof(containerClient));
            EnsureArg.IsNotNull(storageCheckpointOptions, nameof(storageCheckpointOptions));
            EnsureArg.IsNotNull(eventHubClientOptions, nameof(eventHubClientOptions));

            (string eventHubNamespaceFQDN, string eventHubName) = GetEventHubProperties(eventHubClientOptions);
            EnsureArg.IsNotNullOrWhiteSpace(eventHubNamespaceFQDN, nameof(eventHubNamespaceFQDN));
            EnsureArg.IsNotNullOrWhiteSpace(eventHubName, nameof(eventHubName));

            // Blob path for checkpoints includes the event hub name to scope the checkpoints per source event hub.
            _blobCheckpointPrefix = $"{storageCheckpointOptions.BlobPrefix}/checkpoint/";
            _blobPath = $"{_blobCheckpointPrefix}{eventHubNamespaceFQDN}/{eventHubName}/";

            _lastCheckpointMaxCount = int.Parse(storageCheckpointOptions.CheckpointBatchCount);
            _checkpoints = new ConcurrentDictionary<string, Checkpoint>();
            _lastCheckpointTracker = new ConcurrentDictionary<string, int>();
            _storageClient = containerClient;
            _log = log;
        }

        public BlobContainerClient GetBlobContainerClient()
        {
            return _storageClient;
        }

        public async Task UpdateCheckpointAsync(Checkpoint checkpoint)
        {
            EnsureArg.IsNotNull(checkpoint);
            EnsureArg.IsNotNullOrWhiteSpace(checkpoint.Id);
            var lastProcessed = EnsureArg.IsNotNullOrWhiteSpace(checkpoint.LastProcessed.DateTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

            var blobName = $"{checkpoint.Prefix}{checkpoint.Id}";
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
            var prefix = $"{_blobPath}{partitionIdentifier}";

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

                    checkpoint.Prefix = _blobPath;
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
                checkpoint.Prefix = _blobPath;

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

        /// <summary>
        /// Deletes the previously recorded checkpoints if the current checkpoint blob path (corresponding to the input event hub) has changed.
        /// </summary>
        public async Task ResetCheckpointsAsync()
        {
            try
            {
                _log.LogTrace($"Entering {nameof(ResetCheckpointsAsync)}...");

                foreach (BlobItem blob in _storageClient.GetBlobs(states: BlobStates.All, prefix: _blobCheckpointPrefix, cancellationToken: CancellationToken.None))
                {
                    if (!blob.Name.Contains(_blobPath, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            await _storageClient.DeleteBlobAsync(blob.Name, cancellationToken: CancellationToken.None);
                            _log.LogTrace($"Blob checkpoint path changed to {_blobPath}. Deleted checkpoint {blob.Name}.");
                        }
#pragma warning disable CA1031
                        catch (Exception ex)
#pragma warning restore CA1031
                        {
                            _log.LogError(new Exception($"Unable to delete checkpoint {blob.Name} with error {ex.Message}"));
                        }
                    }
                }

                _log.LogTrace($"Exiting {nameof(ResetCheckpointsAsync)}.");
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _log.LogError(new Exception($"Unable to reset checkpoints. {ex.Message}"));
            }
        }

        private (string eventHubNamespaceFQDN, string eventHubName) GetEventHubProperties(EventHubClientOptions eventHubClientOptions)
        {
            // If the authentication type for the event hub is ConnectionString, then parse the event hub properties (eventHubNamspaceFQDN and eventHubName) from the provided connection string,
            // else return the supplied eventHubClientOptions properties for eventHubNamspaceFQDN and eventHubOptions.
            var eventHubNamespaceFQDN = eventHubClientOptions.EventHubNamespaceFQDN;
            var eventHubName = eventHubClientOptions.EventHubName;

            if (eventHubClientOptions.AuthenticationType == AuthenticationType.ConnectionString)
            {
                EnsureArg.IsNotNull(eventHubClientOptions.ConnectionString, nameof(eventHubClientOptions.ConnectionString));

                try
                {
                    var eventHubsConnectionStringProperties = EventHubsConnectionStringProperties.Parse(eventHubClientOptions.ConnectionString);
                    eventHubNamespaceFQDN = eventHubsConnectionStringProperties.FullyQualifiedNamespace;
                    eventHubName = eventHubsConnectionStringProperties.EventHubName;
                }
#pragma warning disable CA1031
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    _log.LogError(new Exception($"Unable to parse event hub properties. {ex.Message}"));
                }
            }

            return (eventHubNamespaceFQDN, eventHubName);
        }
    }
}
