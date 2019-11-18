// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Health.Fhir.Ingest.Service;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class MeasurementToEventAsyncCollector :
        IAsyncCollector<IMeasurement>
    {
        private readonly IEventHubService _eventHubService;

        public MeasurementToEventAsyncCollector(IEventHubService eventHubService)
        {
            _eventHubService = EnsureArg.IsNotNull(eventHubService, nameof(eventHubService));
        }

        public async Task AddAsync(IMeasurement item, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureArg.IsNotNull(item, nameof(item));

            var partitionKey = Ensure.String.IsNotNullOrWhiteSpace(item.DeviceId, nameof(item.DeviceId));
            var measurementContent = JsonConvert.SerializeObject(item, Formatting.None);
            var contentBytes = Encoding.UTF8.GetBytes(measurementContent);

            using (var eventData = new EventData(contentBytes))
            {
                await _eventHubService.SendAsync(eventData, partitionKey).ConfigureAwait(false);
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Currently no batching, no flush needed at this time.
            await Task.Yield();
        }
    }
}
