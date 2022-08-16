﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
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
using Microsoft.Health.Common.Extension;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Events.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventCheckpointing
{
    public class StorageCheckpointClient : ICheckpointClient
    {
        private readonly string _blobCheckpointPrefix;
        private readonly string _blobPath;
        private readonly ITelemetryLogger _logger;
        private readonly BlobContainerClient _storageClient;
        private const string LastProcessedKey = "LastProcessed";
        private const string SequenceNumberKey = "SequenceNumber";
        private const string OffsetKey = "Offset";

        public StorageCheckpointClient(BlobContainerClient containerClient, StorageCheckpointOptions storageCheckpointOptions, EventHubClientOptions eventHubClientOptions, ITelemetryLogger logger)
        {
            _storageClient = EnsureArg.IsNotNull(containerClient, nameof(containerClient));
            EnsureArg.IsNotNull(storageCheckpointOptions, nameof(storageCheckpointOptions));
            EnsureArg.IsNotNull(eventHubClientOptions, nameof(eventHubClientOptions));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));

            (string eventHubNamespaceFQDN, string eventHubName) = GetEventHubProperties(eventHubClientOptions);
            EnsureArg.IsNotNullOrWhiteSpace(eventHubNamespaceFQDN, nameof(eventHubNamespaceFQDN));
            EnsureArg.IsNotNullOrWhiteSpace(eventHubName, nameof(eventHubName));

            // Blob path for checkpoints includes the event hub name to scope the checkpoints per source event hub.
            _blobCheckpointPrefix = $"{storageCheckpointOptions.BlobPrefix}/checkpoint/";
            _blobPath = $"{_blobCheckpointPrefix}{eventHubNamespaceFQDN}/{eventHubName}/";
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
                { LastProcessedKey,  lastProcessed },
                { SequenceNumberKey,  checkpoint.SequenceNumber.ToString() },
                { OffsetKey,  checkpoint.Offset.ToString() },
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
            catch (Exception ex)
            {
                _logger.LogError(new StorageCheckpointClientException($"Unable to update checkpoint. {ex.Message}", ex));
            }
        }

        public async Task<Checkpoint> GetCheckpointForPartitionAsync(string partitionIdentifier, CancellationToken cancellationToken)
        {
            var patitionCheckpointFileBlobPath = $"{_blobPath}{partitionIdentifier}";

            try
            {
                var requestConditions = new BlobRequestConditions();
                var propsResponse = await _storageClient.GetBlobClient(patitionCheckpointFileBlobPath).GetPropertiesAsync(requestConditions, cancellationToken);
                var props = propsResponse.Value;

                DateTimeOffset lastEventTimestamp = DateTime.MinValue;
                long sequenceNumber = -1;
                long offset = -1;

                if (props.Metadata.TryGetValue(LastProcessedKey, out var str))
                {
                    DateTimeOffset.TryParse(str, null, DateTimeStyles.AssumeUniversal, out lastEventTimestamp);
                }

                if (props.Metadata.TryGetValue(SequenceNumberKey, out var sequenceNumberString))
                {
                    long.TryParse(sequenceNumberString, out sequenceNumber);
                }

                if (props.Metadata.TryGetValue(OffsetKey, out var offsetString))
                {
                    long.TryParse(offsetString, out offset);
                }

                var checkpoint = new Checkpoint();
                checkpoint.Prefix = _blobPath;
                checkpoint.Id = partitionIdentifier;
                checkpoint.LastProcessed = lastEventTimestamp;
                checkpoint.SequenceNumber = sequenceNumber;
                checkpoint.Offset = offset;

                return checkpoint;
            }
            catch (RequestFailedException e) when (e.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // No checkpoint exists yet for this partition. This can happen when a new IotConnector has not yet
                // processed any messages. Ignore
                _logger.LogTrace($"The checkpoint file {partitionIdentifier} does not exist");
                return new Checkpoint();
            }
            catch (Exception ex)
            {
                _logger.LogError(new StorageCheckpointClientException($"Unable to get checkpoint for partition. {ex.Message}", ex));
                throw;
            }
        }

        public async Task SetCheckpointAsync(IEventMessage eventArgs, IEnumerable<KeyValuePair<Metric, double>> metrics = null)
        {
            EnsureArg.IsNotNull(eventArgs);
            EnsureArg.IsNotNullOrWhiteSpace(eventArgs.PartitionId);

            try
            {
                var partitionId = eventArgs.PartitionId;
                var checkpoint = new Checkpoint
                {
                    LastProcessed = eventArgs.EnqueuedTime,
                    Id = partitionId,
                    Prefix = _blobPath,
                    SequenceNumber = eventArgs.SequenceNumber,
                    Offset = eventArgs.Offset,
                };

                await UpdateCheckpointAsync(checkpoint);

                _logger.LogMetric(EventMetrics.EventWatermark(partitionId), 1);

                if (metrics != null)
                {
                    foreach (var metric in metrics)
                    {
                        _logger.LogMetric(metric.Key, metric.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(new StorageCheckpointClientException($"Unable to set checkpoint. {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Deletes the previously recorded checkpoints if the current checkpoint blob path (corresponding to the input event hub) has changed.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns><see cref="Task"/></returns>
        public async Task ResetCheckpointsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogTrace($"Entering {nameof(ResetCheckpointsAsync)}...");

                var hasEventHubChanged = false;

                var blobs = _storageClient.GetBlobs(states: BlobStates.All, prefix: _blobCheckpointPrefix, cancellationToken: cancellationToken);

                foreach (BlobItem blob in blobs)
                {
                    if (!blob.Name.Contains(_blobPath, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            await _storageClient.DeleteBlobAsync(blob.Name, cancellationToken: cancellationToken);
                            _logger.LogTrace($"Blob checkpoint path changed to {_blobPath}. Deleted checkpoint {blob.Name}.");
                            hasEventHubChanged = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(new StorageCheckpointClientException($"Unable to delete checkpoint {blob.Name} with error {ex.Message}", ex));
                        }
                    }
                }

                if (blobs.Count() == 0 || hasEventHubChanged)
                {
                    _logger.LogMetric(EventMetrics.EventHubChanged(_blobPath.Replace(_blobCheckpointPrefix, string.Empty)), 1);
                }

                _logger.LogTrace($"Exiting {nameof(ResetCheckpointsAsync)}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(new StorageCheckpointClientException($"Unable to reset checkpoints. {ex.Message}", ex));
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
                catch (Exception ex)
                {
                    _logger.LogError(new StorageCheckpointClientException($"Unable to parse event hub properties. {ex.Message}", ex));
                }
            }

            return (eventHubNamespaceFQDN, eventHubName);
        }
    }
}
