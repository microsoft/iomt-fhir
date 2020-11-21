using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Events.Storage;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;
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

            var eventHub = config.GetSection("Console:EventHub").Value;
            var eventHubOptions = GetEventHubInfo(config, eventHub);

            EnsureArg.IsNotNullOrWhiteSpace(eventHubOptions.EventHubConnectionString);
            EnsureArg.IsNotNullOrWhiteSpace(eventHubOptions.EventHubName);

            var eventBatchingOptions = new EventBatchingOptions();
            config.GetSection(EventBatchingOptions.Settings).Bind(eventBatchingOptions);

            var storageOptions = new StorageOptions();
            config.GetSection(StorageOptions.Settings).Bind(storageOptions);

            var serviceProvider = GetRequiredServiceProvider(config, eventHub);
            var eventConsumers = GetEventConsumers(config, eventHub, serviceProvider);

            var eventConsumerService = new EventConsumerService(eventConsumers);
            var checkpointClient = new StorageCheckpointClient(storageOptions);

            var ct = new CancellationToken();

            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            BlobContainerClient storageClient = new BlobContainerClient(storageOptions.BlobStorageConnectionString, storageOptions.BlobContainerName);

            var eventProcessorClientOptions = new EventProcessorClientOptions();
            eventProcessorClientOptions.MaximumWaitTime = TimeSpan.FromSeconds(60);
            EventProcessorClient client = new EventProcessorClient(storageClient, consumerGroup, eventHubOptions.EventHubConnectionString, eventHubOptions.EventHubName, eventProcessorClientOptions);

            var eventBatchingService = new EventBatchingService(eventConsumerService, eventBatchingOptions, checkpointClient);
            var eventHubReader = new EventProcessor(eventBatchingService, checkpointClient);
            await eventHubReader.RunAsync(client, ct);
        }

        public static IConfiguration GetEnvironmentConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            return config;
        }

        public static ServiceProvider GetRequiredServiceProvider(IConfiguration config, string eventHub)
        {
            if (eventHub == "devicedata")
            {
                var serviceCollection = new ServiceCollection();
                Normalize.ProcessorStartup startup = new Normalize.ProcessorStartup(config);
                startup.ConfigureServices(serviceCollection);
                var serviceProvider = serviceCollection.BuildServiceProvider();
                return serviceProvider;
            }
            else if (eventHub == "normalizeddata")
            {
                var serviceCollection = new ServiceCollection();
                MeasurementCollectionToFhir.ProcessorStartup startup = new MeasurementCollectionToFhir.ProcessorStartup(config);
                startup.ConfigureServices(serviceCollection);
                var serviceProvider = serviceCollection.BuildServiceProvider();
                return serviceProvider;
            }
            else
            {
                throw new Exception("No valid event hub type was found");
            }
        }

        public static EventHubOptions GetEventHubInfo(IConfiguration config, string eventHub)
        {
            var connectionString = eventHub == "devicedata"
                ? config.GetSection("InputEventHub").Value
                : config.GetSection("OutputEventHub").Value;

            var eventHubName = connectionString.Substring(connectionString.LastIndexOf('=') + 1);
            return new EventHubOptions(connectionString, eventHubName);
        }

        public static List<IEventConsumer> GetEventConsumers(IConfiguration config, string inputEventHub, ServiceProvider sp)
        {
            var eventConsumers = new List<IEventConsumer>();
            if (inputEventHub == "devicedata")
            {
                var template = config.GetSection("Template:DeviceContent").Value;
                var deviceDataNormalization = new Normalize.Processor(template, config, sp.GetRequiredService<IOptions<EventHubMeasurementCollectorOptions>>());
                eventConsumers.Add(deviceDataNormalization);
            }

            else if (inputEventHub == "normalizeddata")
            {
                var template = config.GetSection("Template:FhirMapping").Value;
                var measurementImportService = ResolveMeasurementService(sp);
                var measurementToFhirConsumer = new MeasurementCollectionToFhir.Processor(template, measurementImportService);
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
