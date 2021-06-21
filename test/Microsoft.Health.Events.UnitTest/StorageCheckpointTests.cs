using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Health.Events.UnitTest
{
    public class StorageCheckpointTests
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly EventHubClientOptions _eventHubClientOptions;
        private readonly ITelemetryLogger _logger;
        private readonly StorageCheckpointOptions _storageCheckpointOptions;

        private readonly string _blobCheckpointPrefix;
        private readonly string _blobPath;
        private readonly string _eventHubName;
        private readonly string _eventHubNamespaceFQDN;

        public StorageCheckpointTests()
        {
            _blobContainerClient = Substitute.For<BlobContainerClient>();

            _storageCheckpointOptions = new StorageCheckpointOptions()
            {
                BlobPrefix = "Normalization",
                CheckpointBatchCount = "5"
            };

            _eventHubClientOptions = new EventHubClientOptions();
            _eventHubNamespaceFQDN = "test.servicebus.windows.net";
            _eventHubName = "devicedata";

            // Blob path corresponds to current event hub name
            _blobCheckpointPrefix = $"{_storageCheckpointOptions.BlobPrefix}/checkpoint/";
            _blobPath = $"{_blobCheckpointPrefix}{_eventHubNamespaceFQDN}/{_eventHubName}/";

            IReadOnlyList<BlobItem> mockBlobItems = new List<BlobItem>()
            {
                BlobsModelFactory.BlobItem(name: $"{_blobPath}1"),
                BlobsModelFactory.BlobItem(name: $"{_blobPath}10"),
                BlobsModelFactory.BlobItem(name: $"{_blobPath}20")
            };

            var mockPageBlobItems = Page<BlobItem>.FromValues(mockBlobItems, "continuationToken", Substitute.For<Response>());
            var mockPageableBlobItems = Pageable<BlobItem>.FromPages(new[] { mockPageBlobItems });

            _blobContainerClient.GetBlobs(states: BlobStates.All, prefix: _blobCheckpointPrefix, cancellationToken: CancellationToken.None)
                .Returns(mockPageableBlobItems);

            _logger = Substitute.For<ITelemetryLogger>();
        }

        [Fact]
        public void GivenNullParameters_WhenStorageCheckpointClientCreated_Throws()
        {
            Assert.Throws<ArgumentNullException>("containerClient", () => new StorageCheckpointClient(null, _storageCheckpointOptions, _eventHubClientOptions, _logger));
            Assert.Throws<ArgumentNullException>("storageCheckpointOptions", () => new StorageCheckpointClient(_blobContainerClient, null, _eventHubClientOptions, _logger));
            Assert.Throws<ArgumentNullException>("eventHubClientOptions", () => new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, null, _logger));

            var eventHubClientOptions = new EventHubClientOptions();
            Assert.Throws<ArgumentNullException>("eventHubNamespaceFQDN", () => new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, eventHubClientOptions, _logger));

            eventHubClientOptions.EventHubNamespaceFQDN = "test";
            Assert.Throws<ArgumentNullException>("eventHubName", () => new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, eventHubClientOptions, _logger));

            eventHubClientOptions = new EventHubClientOptions
            {
                AuthenticationType = AuthenticationType.ConnectionString,
                ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=test123;"
            };
            Assert.Throws<ArgumentNullException>("eventHubName", () => new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, eventHubClientOptions, _logger));

            eventHubClientOptions.ConnectionString = "SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=test123;EntityPath=devicedata";
            Assert.Throws<ArgumentNullException>("eventHubNamespaceFQDN", () => new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, eventHubClientOptions, _logger));
        }

        [Fact]
        public async Task GivenUnchangedEventHubOptions_WhenResetCheckpointsAsyncCalled_ThenNoCheckpointsAreDeleted()
        {
            _eventHubClientOptions.EventHubNamespaceFQDN = _eventHubNamespaceFQDN;
            _eventHubClientOptions.EventHubName = _eventHubName;

            var storageClient = new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, _eventHubClientOptions, _logger);
            await storageClient.ResetCheckpointsAsync();

            // Given that the source event hub didn't change, verify that no checkpoint deletions occured.
            _blobContainerClient.Received(1).GetBlobs(states: BlobStates.All, prefix: _blobCheckpointPrefix, cancellationToken: CancellationToken.None);
            await _blobContainerClient.ReceivedWithAnyArgs(0).DeleteBlobAsync(null);
        }

        [Fact]
        public async Task GivenUnchangedEventHubOptionsWithConnectionString_WhenResetCheckpointsAsyncCalled_ThenNoCheckpointsAreDeleted()
        {
            var eventHubClientOptions = new EventHubClientOptions()
            {
                AuthenticationType = AuthenticationType.ConnectionString,
                ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=test123;EntityPath=devicedata"
            };

            var storageClient = new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, eventHubClientOptions, _logger);
            await storageClient.ResetCheckpointsAsync();

            // Given that the source event hub didn't change, verify that no checkpoint deletions occured.
            _blobContainerClient.Received(1).GetBlobs(states: BlobStates.All, prefix: _blobCheckpointPrefix, cancellationToken: CancellationToken.None);
            await _blobContainerClient.ReceivedWithAnyArgs(0).DeleteBlobAsync(null);
        }

        [Fact]
        public async Task GivenUpdatedEventHubNamespace_WhenResetCheckpointsAsyncCalled_ThenPreviousCheckpointsAreDeleted()
        {
            _eventHubClientOptions.EventHubNamespaceFQDN = "newtest.servicebus.windows.net";
            _eventHubClientOptions.EventHubName = _eventHubName;

            var storageClient = new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, _eventHubClientOptions, _logger);
            await storageClient.ResetCheckpointsAsync();

            // Given that the event hub namespace changed and is therefore a new source, verify that the checkpoints corresponding to the old source will be deleted.
            _blobContainerClient.Received(1).GetBlobs(states: BlobStates.All, prefix: _blobCheckpointPrefix, cancellationToken: CancellationToken.None);
            await _blobContainerClient.ReceivedWithAnyArgs(3).DeleteBlobAsync(null);
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{_blobPath}1");
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{_blobPath}10");
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{_blobPath}20");
        }

        [Fact]
        public async Task GivenUpdatedEventHubName_WhenResetCheckpointsAsyncCalled_ThenPreviousCheckpointsAreDeleted()
        {
            _eventHubClientOptions.EventHubNamespaceFQDN = _eventHubNamespaceFQDN;
            _eventHubClientOptions.EventHubName = "newdevicedata";

            var storageClient = new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, _eventHubClientOptions, _logger);
            await storageClient.ResetCheckpointsAsync();

            // Given that the event hub changed and is therefore a new source, verify that the checkpoints corresponding to the old source will be deleted.
            _blobContainerClient.Received(1).GetBlobs(states: BlobStates.All, prefix: _blobCheckpointPrefix, cancellationToken: CancellationToken.None);
            await _blobContainerClient.ReceivedWithAnyArgs(3).DeleteBlobAsync(null);
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{_blobPath}1");
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{_blobPath}10");
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{_blobPath}20");
        }

        [Fact]
        public async Task GivenUpdatedEventHubNamespaceAndEventHubName_WhenResetCheckpointsAsyncCalled_ThenPreviousCheckpointsAreDeleted()
        {
            _eventHubClientOptions.EventHubNamespaceFQDN = "newtest.servicebus.windows.net";
            _eventHubClientOptions.EventHubName = "newdevicedata";

            var storageClient = new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, _eventHubClientOptions, _logger);
            await storageClient.ResetCheckpointsAsync();

            // Given that the event hub namespace and the event hub name changed and is therefore a new source, verify that the checkpoints corresponding to the old source will be deleted.
            _blobContainerClient.Received(1).GetBlobs(states: BlobStates.All, prefix: _blobCheckpointPrefix, cancellationToken: CancellationToken.None);
            await _blobContainerClient.ReceivedWithAnyArgs(3).DeleteBlobAsync(null);
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{_blobPath}1");
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{_blobPath}10");
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{_blobPath}20");
        }

        [Fact]
        public async Task GivenDifferentAppType_WhenResetCheckpointsAsyncCalled_ThenCheckpointsOfOtherAppsAreNotDeleted()
        {
            _eventHubClientOptions.EventHubNamespaceFQDN = _eventHubNamespaceFQDN;
            _eventHubClientOptions.EventHubName = "newdevicedata";
            _storageCheckpointOptions.BlobPrefix = "MeasurementToFhir";
            var fhirconvBlobCheckpointPrefix = $"{_storageCheckpointOptions.BlobPrefix }/checkpoint/";
            var fhirconvBlobPath = $"{fhirconvBlobCheckpointPrefix}{_eventHubNamespaceFQDN}/{_eventHubName}/";

            IReadOnlyList<BlobItem> mockBlobItems = new List<BlobItem>()
            {
                BlobsModelFactory.BlobItem(name: $"{fhirconvBlobPath}1"),
                BlobsModelFactory.BlobItem(name: $"{fhirconvBlobPath}10"),
                BlobsModelFactory.BlobItem(name: $"{fhirconvBlobPath}20")
            };

            var mockPageBlobItems = Page<BlobItem>.FromValues(mockBlobItems, "continuationToken", Substitute.For<Response>());
            var mockPageableBlobItems = Pageable<BlobItem>.FromPages(new[] { mockPageBlobItems });

            _blobContainerClient.GetBlobs(states: BlobStates.All, prefix: fhirconvBlobCheckpointPrefix, cancellationToken: CancellationToken.None)
                .Returns(mockPageableBlobItems);

            var storageClient = new StorageCheckpointClient(_blobContainerClient, _storageCheckpointOptions, _eventHubClientOptions, _logger);
            await storageClient.ResetCheckpointsAsync();

            // Given that we are processing events for a different app type and the source changed, verify that the previous checkpoints corresponding to this app are deleted.
            _blobContainerClient.Received(1).GetBlobs(states: BlobStates.All, prefix: fhirconvBlobCheckpointPrefix, cancellationToken: CancellationToken.None);
            await _blobContainerClient.ReceivedWithAnyArgs(3).DeleteBlobAsync(null);
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{fhirconvBlobPath}1");
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{fhirconvBlobPath}10");
            await _blobContainerClient.Received(1).DeleteBlobAsync($"{fhirconvBlobPath}20");

            // Given that we are processing events for a different app type, verify that the checkpoints corresponding to the other apps are not deleted.
            _blobContainerClient.Received(0).GetBlobs(states: BlobStates.All, prefix: _blobCheckpointPrefix, cancellationToken: CancellationToken.None);
            await _blobContainerClient.Received(0).DeleteBlobAsync($"{_blobPath}1");
            await _blobContainerClient.Received(0).DeleteBlobAsync($"{_blobPath}10");
            await _blobContainerClient.Received(0).DeleteBlobAsync($"{_blobPath}20");
        }
    }
}
