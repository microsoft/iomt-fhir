﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Events.EventProducers;
using Microsoft.Health.Logging.Telemetry;

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
            // if assigned partition processor is enabled, then inject it
            var partitionLockingOptions = new PartitionLockingServiceOptions();
            config.GetSection("PartitionLocking").Bind(partitionLockingOptions);
            var partitionLockingEnabled = partitionLockingOptions.Enabled;

            if (partitionLockingEnabled)
            {
                services.AddSingleton<ICheckpointClient, StorageCheckpointClient>();

                services.AddSingleton(sp =>
                {
                    var eventBatchingServiceOptions = new EventBatchingOptions();
                    config.GetSection(EventBatchingOptions.Settings).Bind(eventBatchingServiceOptions);
                    return eventBatchingServiceOptions;
                });

                services.AddSingleton<EventBatchingService>();

                services.AddSingleton((sp) =>
                {
                    partitionLockingOptions.StorageTokenCredential = sp.GetService<IAzureCredentialProvider>();
                    return partitionLockingOptions;
                });

                services.AddSingleton((sp) =>
                {
                    var options = sp.GetService<EventHubClientOptions>();
                    var eventHubConsumerClient = new EventHubConsumerClient(
                        options?.EventHubConsumerGroup,
                        options?.EventHubNamespaceFQDN,
                        options?.EventHubName,
                        options?.EventHubTokenCredential);

                    return eventHubConsumerClient;
                });

                services.AddSingleton<IProcessorIdProvider>((sp) =>
                {
                    var processorId = Environment.GetEnvironmentVariable("HOSTNAME");
                    processorId ??= Guid.NewGuid().ToString();
                    return new ProcessorIdProvider(processorId);
                });

                services.AddSingleton<IAssignedPartitionProcessorFactory, AssignedPartitionProcessorFactory>();

                services.AddSingleton<IPartitionCoordinator>((sp) =>
                {
                    var logger = sp.GetService<ITelemetryLogger>();
                    var partitionLockingOptions = sp.GetService<PartitionLockingServiceOptions>();
                    var tokenCredential = partitionLockingOptions?.StorageTokenCredential;
                    var containerClient = new BlobContainerClient(partitionLockingOptions?.BlobContainerUri, tokenCredential?.GetCredential());
                    return new PartitionCoordinator(containerClient, logger);
                });

                services.AddSingleton<PartitionLockingService>();
                services.AddSingleton<IResumableEventProcessor, ResumableAssignedPartitionProcessor>();

                return services;
            }

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
