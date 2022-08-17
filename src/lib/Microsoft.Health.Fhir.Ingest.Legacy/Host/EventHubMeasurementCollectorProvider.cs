// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public class EventHubMeasurementCollectorProvider : IExtensionConfigProvider
    {
        private readonly IOptions<EventHubMeasurementCollectorOptions> _options;
        private readonly ILoggerFactory _loggerFactory;

        public EventHubMeasurementCollectorProvider(
            IOptions<EventHubMeasurementCollectorOptions> options,
            ILoggerFactory loggerFactory)
        {
            _options = options;
            _loggerFactory = loggerFactory;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.AddBindingRule<EventHubMeasurementCollectorAttribute>()
                .BindToInput(attr => CreateCollector(attr.EventHubName, attr.Connection));
        }

        private IAsyncCollector<IMeasurement> CreateCollector(string eventHubName, string connectionString)
        {
            var logger = _loggerFactory?.CreateLogger(LogCategories.Executor);
            logger?.LogTrace($"Instantiating Measurement Collector for event hub {eventHubName}.");

            var client = _options.Value.GetEventHubClient(eventHubName, connectionString);
            return new MeasurementToEventAsyncCollector(new EventHubService(client));
        }
    }
}