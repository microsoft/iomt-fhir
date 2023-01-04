// --------------------------------------------------------------------------
// <copyright file="EventProcessorExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Events.EventProducers;
using Azure.Storage.Blobs;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Logging.Telemetry;
using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class EventProcessingExtensions
    {
        public static IServiceCollection AddEventProcessorClientFactory(this IServiceCollection services)
        {
            services.AddSingleton<IEventProcessorClientFactory, EventProcessorClientFactory>();
            return services;
        }

        public static IServiceCollection AddEventConsumerService(this IServiceCollection services)
        {
            services.AddSingleton<IEventConsumerService, EventConsumerService>();
            return services;
        }

        public static IServiceCollection AddEventHubConsumerClientFactory(this IServiceCollection services)
        {
            services.AddSingleton<IEventHubConsumerClientFactory, EventHubConsumerClientFactory>();
            return services;
        }

        public static IServiceCollection AddEventProducerFactory(this IServiceCollection services)
        {
            services.AddSingleton<IEventProducerClientFactory, EventProducerClientFactory>();
            return services;
        }

        public static IServiceCollection AddEventCheckpointing(this IServiceCollection services)
        {
            services.AddSingleton((sp) =>
            {
                var storageOptions = sp.GetRequiredService<StorageCheckpointOptions>();
                var eventProcessorOptions = sp.GetRequiredService<EventHubClientOptions>();
                var logger = sp.GetRequiredService<ITelemetryLogger>();
                var checkpointBlobClient = sp.GetRequiredService<BlobContainerClient>();
                return new StorageCheckpointClient(checkpointBlobClient, storageOptions, eventProcessorOptions, logger);
            });

            return services;
        }

        public static IServiceCollection AddResumableEventProcessor(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IResumableEventProcessor>((sp) =>
            {
                var eventConsumerService = sp.GetRequiredService<IEventConsumerService>();
                var checkpointClient = sp.GetRequiredService<StorageCheckpointClient>();
                var logger = sp.GetRequiredService<ITelemetryLogger>();
                var meters = sp.GetService<IEventProcessingMetricMeters>();
                var eventBatchingOptions = new EventBatchingOptions();
                config.GetSection(EventBatchingOptions.Settings).Bind(eventBatchingOptions);
                var eventBatchingService = new EventBatchingService(eventConsumerService, eventBatchingOptions, checkpointClient, logger, meters);

                var eventProcessorClient = sp.GetRequiredService<EventProcessorClient>();
                return new ResumableEventProcessor(eventProcessorClient, eventBatchingService, checkpointClient, logger);
            });

            return services;
        }
    }
}
