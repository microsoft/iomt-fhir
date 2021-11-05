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
    public class MeasurementToEventMessageAsyncCollector :
        IEnumerableAsyncCollector<IMeasurement>
    {
        private readonly IEventHubMessageService _eventHubService;
        private readonly IHashCodeFactory _hashCodeFactory;
        private readonly ITelemetryLogger _telemetryLogger;

        public MeasurementToEventMessageAsyncCollector(
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

                    foreach (var m in grp)
                    {
                        var measurementContent = JsonConvert.SerializeObject(m, Formatting.None);
                        var contentBytes = Encoding.UTF8.GetBytes(measurementContent);
                        var eventData = new EventData(contentBytes);

                        if (!currentEventDataBatch.TryAdd(eventData))
                        {
                            // The current EventDataBatch cannot hold any more events. Create a new EventDataBatch and add this new message to it.
                            var newEventDataBatch = await _eventHubService.CreateEventDataBatchAsync(partitionKey);

                            if (!newEventDataBatch.TryAdd(eventData))
                            {
                                // The measurement event is greater than the size allowed by EventHub. Log and discard. Keep the existing batch as there may
                                // be room for more events.
                                // TODO in this case we should send this to a dead letter queue. We'd need to see how we can send it, as it is too big for EventHub...
                                _telemetryLogger.LogError(new ArgumentOutOfRangeException($"A measurement event exceeded the maximum message batch size of {newEventDataBatch.MaximumSizeInBytes} bytes. It will be skipped."));
                            }
                            else
                            {
                                // Submit the current batch, and replace the currentEventDataBatch with newEventDataBatch
                                await _eventHubService.SendAsync(currentEventDataBatch, cancellationToken);
                                currentEventDataBatch.Dispose();
                                currentEventDataBatch = newEventDataBatch;
                            }
                        }
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
