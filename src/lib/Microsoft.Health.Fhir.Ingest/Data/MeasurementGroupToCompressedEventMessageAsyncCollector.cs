// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class MeasurementGroupToCompressedEventMessageAsyncCollector :
        IEnumerableAsyncCollector<IMeasurement>
    {
        private readonly IEventHubMessageService _eventHubService;
        private readonly IHashCodeFactory _hashCodeFactory;
        private readonly ITelemetryLogger _telemetryLogger;

        public MeasurementGroupToCompressedEventMessageAsyncCollector(
            IEventHubMessageService eventHubService,
            IHashCodeFactory hashCodeFactory,
            ITelemetryLogger telemetryLogger)
        {
            _eventHubService = EnsureArg.IsNotNull(eventHubService, nameof(eventHubService));
            _hashCodeFactory = EnsureArg.IsNotNull(hashCodeFactory, nameof(hashCodeFactory));
            _telemetryLogger = EnsureArg.IsNotNull(telemetryLogger, nameof(telemetryLogger));
        }

        public async Task AddAsync(IMeasurement item, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureArg.IsNotNull(item, nameof(item));

            var partitionKey = Ensure.String.IsNotNullOrWhiteSpace(item.DeviceId, nameof(item.DeviceId));
            var measurementContent = JsonConvert.SerializeObject(item, Formatting.None);
            var contentBytes = Encoding.UTF8.GetBytes(measurementContent);

            var eventList = new List<EventData>();
            var eventData = new EventData(contentBytes);
            eventList.Add(eventData);
            await _eventHubService.SendAsync(eventList, partitionKey, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddAsync(IEnumerable<IMeasurement> items, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(items, nameof(items));

            using (var hasher = _hashCodeFactory.CreateDeterministicHashCodeGenerator())
            {
                var submissionTasks = items
                .GroupBy(m =>
                {
                    // cast as byte to restrict to 256 possible values. This will lead to a greater change of measurements ending up in the same bucket,
                    // while providing partition keys with enough entropy for EventHub to better distribute them across partitions.
                    return hasher.GenerateHashCode(m.DeviceId.ToLower());
                })
                .Select(async grp =>
                {
                    var partitionKey = grp.Key;
                    var currentEventDataBatch = await _eventHubService.CreateEventDataBatchAsync(partitionKey);

                    // Compression enhancement
                    // Behavior is to compress the entire measurement group and send it
                    // When decompression is supported on the fhirtransformation side,
                    // the compression enhancement class will be injected at startup by default
                    var measurementGroupContent = JsonConvert.SerializeObject(grp, Formatting.None);
                    var compressedBytes = Common.IO.Compression.CompressWithGzip(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(grp));
                    var eventData = new EventData(compressedBytes);
                    eventData.ContentType = Common.IO.Compression.GzipContentType;
                    eventData.Properties["IsMeasurementGroup"] = true;

                    if (!currentEventDataBatch.TryAdd(eventData))
                    {
                        _telemetryLogger.LogError(new ArgumentOutOfRangeException($"A measurement group exceeded the maximum message batch size of {currentEventDataBatch.MaximumSizeInBytes} bytes. It will be skipped."));

                        // consider sending to error message service
                    }

                    // Send over the remaining events
                    await _eventHubService.SendAsync(currentEventDataBatch, cancellationToken);
                });

                await Task.WhenAll(submissionTasks);
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Yield();
        }
    }
}
