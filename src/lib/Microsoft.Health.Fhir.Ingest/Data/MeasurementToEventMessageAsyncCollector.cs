// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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
    public class MeasurementToEventMessageAsyncCollector : IBatchingAsyncCollector<IMeasurement>
    {
        private readonly IEventHubMessageService _eventHubService;
        private readonly ITelemetryLogger _log;

        public MeasurementToEventMessageAsyncCollector(
            IEventHubMessageService eventHubService,
            ITelemetryLogger log)
        {
            _eventHubService = EnsureArg.IsNotNull(eventHubService, nameof(eventHubService));
            _log = EnsureArg.IsNotNull(log, nameof(log));
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

        public async Task AddAsync(IEnumerable<IMeasurement> measurements, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(measurements, nameof(measurements));
            var submissionTasks = measurements
                .GroupBy(m => m.DeviceId)
                .Select(async grp =>
                {
                    var partitionKey = EnsureArg.IsNotNullOrWhiteSpace(grp.Key, "DeviceId");
                    var eventList = new List<EventData>();
                    foreach (var m in grp)
                    {
                        var measurementContent = JsonConvert.SerializeObject(m, Formatting.None);
                        var contentBytes = Encoding.UTF8.GetBytes(measurementContent);
                        var eventData = new EventData(contentBytes);
                        eventList.Add(eventData);
                    }

                    _log.LogTrace($"Submitting {measurements.Count()} batched events for partition {partitionKey}");
                    await _eventHubService.SendAsync(eventList, partitionKey, cancellationToken).ConfigureAwait(false);
                });

            await Task.WhenAll(submissionTasks);
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Currently no batching, no flush needed at this time.
            await Task.Yield();
        }
    }
}
