// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Service;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class MeasurementToEventMessageAsyncCollector :
        IEnumerableAsyncCollector<IMeasurement>
    {
        private readonly IEventHubMessageService _eventHubService;
        private readonly int _partitionCount;
        private readonly IHashCodeFactory _hashCodeFactory;

        public MeasurementToEventMessageAsyncCollector(
            IEventHubMessageService eventHubService,
            IHashCodeFactory hashCodeFactory,
            int partitionCount = 16)
        {
            _eventHubService = EnsureArg.IsNotNull(eventHubService, nameof(eventHubService));
            _partitionCount = EnsureArg.IsGt(partitionCount, 0, nameof(partitionCount));
            _hashCodeFactory = EnsureArg.IsNotNull(hashCodeFactory, nameof(hashCodeFactory));
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
                    var partitionKey = hasher.GenerateHashCode(m.DeviceId.ToLower()) % _partitionCount;
                    var partitionKeyAsString = partitionKey.ToString(CultureInfo.InvariantCulture);
                    return partitionKeyAsString;
                })
                .Select(async grp =>
                {
                    var partitionKey = grp.Key;
                    var eventList = new List<EventData>();
                    foreach (var m in grp)
                    {
                        var measurementContent = JsonConvert.SerializeObject(m, Formatting.None);
                        var contentBytes = Encoding.UTF8.GetBytes(measurementContent);
                        var eventData = new EventData(contentBytes);
                        eventList.Add(eventData);
                    }

                    await _eventHubService.SendAsync(eventList, partitionKey, cancellationToken).ConfigureAwait(false);
                });

                await Task.WhenAll(submissionTasks);
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Currently no batching, no flush needed at this time.
            await Task.Yield();
        }
    }
}
