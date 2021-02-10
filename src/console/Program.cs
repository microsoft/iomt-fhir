// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Common.Storage;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Events.Repository;
using Microsoft.Health.Fhir.Ingest.Console.Template;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Logging.Telemetry;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Ingest.Console
{
    public class Program
    {
        public static async Task Main()
        {
            var config = GetEnvironmentConfig();
            var eventHubName = GetEventHubName(config);

            var serviceCollection = GetRequiredServiceCollection(config, eventHubName);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ITelemetryLogger>();

            var blobContainerClientFactory = new BlobContainerClientFactory();
            var eventConsumers = GetEventConsumers(config, eventHubName, serviceProvider, blobContainerClientFactory, logger);

            var storageOptions = new StorageCheckpointOptions();
            config.GetSection(StorageCheckpointOptions.Settings).Bind(storageOptions);
            var checkpointContainerOptions = new BlobContainerClientOptions();
            config.GetSection("CheckpointStorage").Bind(checkpointContainerOptions);

            var storageCheckpointClient = GetStorageCheckpointClient(blobContainerClientFactory, checkpointContainerOptions, storageOptions, logger, eventHubName);
            var eventConsumerService = new EventConsumerService(eventConsumers, logger);

            var eventProcessorOptions = GetEventProcessorFactoryOptions(config);
            var eventProcessorClientFactory = new EventProcessorClientFactory(storageCheckpointClient.GetBlobContainerClient());
            var eventProcessorClientOptions = new EventProcessorClientOptions();
            eventProcessorClientOptions.MaximumWaitTime = TimeSpan.FromSeconds(60);
            var incomingEventReader = eventProcessorClientFactory.CreateProcessorClient(eventProcessorOptions, eventProcessorClientOptions);
            var eventHubReader = GetEventProcessor(config, eventConsumerService, storageCheckpointClient, logger);

            System.Console.WriteLine($"Reading from event hub: {eventHubName}");
            var ct = new CancellationToken();
            await eventHubReader.RunAsync(incomingEventReader, ct);
        }

        public static string GetEventHubName(IConfiguration config)
        {
            var eventHub = Environment.GetEnvironmentVariable("WEBJOBS_NAME");
            if (string.IsNullOrWhiteSpace(eventHub))
            {
                eventHub = config.GetSection("Console:EventHub").Value;
            }

            return eventHub;
        }

        public static StorageCheckpointClient GetStorageCheckpointClient(BlobContainerClientFactory factory, BlobContainerClientOptions containerOptions, StorageCheckpointOptions options, ITelemetryLogger logger, string prefix)
        {
            var checkpointBlobClient = factory.CreateStorageClient(containerOptions);

            options.BlobPrefix = prefix;
            var checkpointClient = new StorageCheckpointClient(checkpointBlobClient, options, logger);
            return checkpointClient;
        }

        public static TemplateManager GetMappingTemplateManager(IConfiguration config, BlobContainerClientFactory blobClientFactory)
        {
            var containerOptions = new BlobContainerClientOptions();
            config.GetSection("TemplateStorage").Bind(containerOptions);
            var containerClient = blobClientFactory.CreateStorageClient(containerOptions);
            var storageManager = new StorageManager(containerClient);
            var templateManager = new TemplateManager(storageManager);
            return templateManager;
        }

        public static EventProcessor GetEventProcessor(
            IConfiguration config,
            IEventConsumerService eventConsumerService,
            ICheckpointClient checkpointClient,
            ITelemetryLogger logger)
        {
            var eventBatchingOptions = new EventBatchingOptions();
            config.GetSection(EventBatchingOptions.Settings).Bind(eventBatchingOptions);
            var eventBatchingService = new EventBatchingService(eventConsumerService, eventBatchingOptions, checkpointClient, logger);
            var eventHubReader = new EventProcessor(eventBatchingService, checkpointClient, logger);
            return eventHubReader;
        }

        public static EventProcessorClientFactoryOptions GetEventProcessorFactoryOptions(IConfiguration config)
        {
            var eventHubName = GetEventHubName(config);
            var eventProcessorOptions = new EventProcessorClientFactoryOptions();

            switch (eventHubName)
            {
                case "devicedata":
                    config.GetSection("InputEventHub").Bind(eventProcessorOptions);
                    break;
                case "normalizeddata":
                    config.GetSection("OutputEventHub").Bind(eventProcessorOptions);
                    break;
                default:
                    throw new Exception("Unable to create EventProcessorClientFactoryOptions from configuration");
            }

            return eventProcessorOptions;
        }

        public static IConfiguration GetEnvironmentConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        public static ServiceCollection GetRequiredServiceCollection(IConfiguration config, string eventHub)
        {
            if (eventHub == "devicedata")
            {
                var serviceCollection = new ServiceCollection();
                ConfigureLogging(config, serviceCollection);
                return serviceCollection;
            }
            else if (eventHub == "normalizeddata")
            {
                var serviceCollection = new ServiceCollection();
                MeasurementCollectionToFhir.ProcessorStartup startup = new MeasurementCollectionToFhir.ProcessorStartup(config);
                startup.ConfigureServices(serviceCollection);

                ConfigureLogging(config, serviceCollection);

                return serviceCollection;
            }
            else
            {
                throw new Exception("No valid event hub type was found");
            }
        }

        public static void ConfigureLogging(IConfiguration config, IServiceCollection serviceCollection)
        {
            var instrumentationKey = config.GetSection("APPINSIGHTS_INSTRUMENTATIONKEY").Value;

            TelemetryConfiguration telemetryConfig;
            TelemetryClient telemetryClient;

            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                telemetryConfig = new TelemetryConfiguration();
                telemetryClient = new TelemetryClient(telemetryConfig);
            }
            else
            {
                telemetryConfig = new TelemetryConfiguration(instrumentationKey);
                telemetryClient = new TelemetryClient(telemetryConfig);
            }

            var logger = new IomtTelemetryLogger(telemetryClient);
            serviceCollection.TryAddSingleton<ITelemetryLogger>(logger);
        }

        public static List<IEventConsumer> GetEventConsumers(IConfiguration config, string inputEventHub, ServiceProvider sp, BlobContainerClientFactory blobClientFactory, ITelemetryLogger logger)
        {
            var eventConsumers = new List<IEventConsumer>();

            var templateManager = GetMappingTemplateManager(config, blobClientFactory);

            if (inputEventHub == "devicedata")
            {
                var template = config.GetSection("Template:DeviceContent").Value;
                var deviceDataNormalization = new Normalize.Processor(template, templateManager, config, logger);
                eventConsumers.Add(deviceDataNormalization);
            }

            else if (inputEventHub == "normalizeddata")
            {
                var template = config.GetSection("Template:FhirMapping").Value;
                var measurementImportService = ResolveMeasurementService(sp);
                var measurementToFhirConsumer = new MeasurementCollectionToFhir.Processor(template, templateManager, measurementImportService, logger);
                eventConsumers.Add(measurementToFhirConsumer);
            }

            if (config.GetSection("Console:Debug")?.Value == "true")
            {
                eventConsumers.Add(new EventPrinter());
            }

            return eventConsumers;
        }

        public static MeasurementFhirImportService ResolveMeasurementService(IServiceProvider services)
        {
            return services.GetRequiredService<MeasurementFhirImportService>();
        }
    }
}
