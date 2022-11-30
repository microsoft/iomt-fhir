// --------------------------------------------------------------------------
// <copyright file="StartupBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using DevLab.JmesPath;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Common.Storage;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Events.EventProducers;
using Microsoft.Health.Expressions;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Console.Common
{
    public abstract class StartupBase
    {
        protected StartupBase()
        {
            Configuration = GetEnvironmentConfig();
            ServiceCollection = new ServiceCollection();
        }

        public ServiceCollection ServiceCollection { get; set; }

        public IConfiguration Configuration { get; }

        public abstract string ApplicationType { get; }

        public abstract string OperationType { get; }

        public static IConfiguration GetEnvironmentConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("local.appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            AddContentTemplateFactories(services);

            services.AddSingleton<IEventProcessorClientFactory, EventProcessorClientFactory>();
            services.AddSingleton<IEventProducerClientFactory, EventProducerClientFactory>();
            services.AddSingleton<IEventHubConsumerClientFactory, EventHubConsumerClientFactory>();
            services.AddSingleton<IEventConsumerService, EventConsumerService>();

            services.AddSingleton<BlobContainerClientFactory>();

            services.AddSingleton((sp) =>
            {
                // Add StorageCheckpointOptions
                var storageOptions = new StorageCheckpointOptions();
                Configuration.GetSection(StorageCheckpointOptions.Settings).Bind(storageOptions);
                storageOptions.BlobPrefix = $"{ApplicationType}/{storageOptions.BlobPrefix}";
                return storageOptions;
            });
            services.AddSingleton((sp) =>
            {
                // Add BlobContainerOptions
                var checkpointContainerOptions = new BlobContainerClientOptions();
                Configuration.GetSection("CheckpointStorage").Bind(checkpointContainerOptions);
                return checkpointContainerOptions;
            });
            services.AddSingleton((sp) =>
            {
                // Add BlobContainerClient
                var factory = sp.GetRequiredService<BlobContainerClientFactory>();
                var checkpointContainerOptions = sp.GetRequiredService<BlobContainerClientOptions>();
                var tokenProvider = sp.GetService<IAzureCredentialProvider>();
                return factory.CreateStorageClient(checkpointContainerOptions, tokenProvider);
            });
            services.AddSingleton((sp) =>
            {
                // Add ICheckpointClient
                var storageOptions = sp.GetRequiredService<StorageCheckpointOptions>();
                var eventProcessorOptions = sp.GetRequiredService<EventHubClientOptions>();
                var logger = sp.GetRequiredService<ITelemetryLogger>();
                var checkpointBlobClient = sp.GetRequiredService<BlobContainerClient>();
                return new StorageCheckpointClient(checkpointBlobClient, storageOptions, eventProcessorOptions, logger);
            });
            services.AddSingleton<IResumableEventProcessor>((sp) =>
            {
                // Add EventProcessor
                var eventConsumerService = sp.GetRequiredService<IEventConsumerService>();
                var checkpointClient = sp.GetRequiredService<StorageCheckpointClient>();
                var logger = sp.GetRequiredService<ITelemetryLogger>();
                var meters = sp.GetService<IEventProcessingMetricMeters>();
                var eventBatchingOptions = new EventBatchingOptions();
                Configuration.GetSection(EventBatchingOptions.Settings).Bind(eventBatchingOptions);
                var eventBatchingService = new EventBatchingService(eventConsumerService, eventBatchingOptions, checkpointClient, logger, meters);

                var eventProcessorClient = sp.GetRequiredService<EventProcessorClient>();
                return new ResumableEventProcessor(eventProcessorClient, eventBatchingService, checkpointClient, logger);
            });
        }

        private static void AddContentTemplateFactories(IServiceCollection services)
        {
            services.AddSingleton<IExpressionRegister>(sp => new AssemblyExpressionRegister(typeof(IExpressionRegister).Assembly, sp.GetRequiredService<ITelemetryLogger>()));
            services.AddSingleton(
                sp =>
                {
                    var jmesPath = new JmesPath();
                    var expressionRegister = sp.GetRequiredService<IExpressionRegister>();
                    expressionRegister.RegisterExpressions(jmesPath.FunctionRepository);
                    return jmesPath;
                });
            services.AddSingleton<IExpressionEvaluatorFactory, TemplateExpressionEvaluatorFactory>();
            services.AddSingleton<ITemplateFactory<TemplateContainer, IContentTemplate>, JsonPathContentTemplateFactory>();
            services.AddSingleton<ITemplateFactory<TemplateContainer, IContentTemplate>, IotJsonPathContentTemplateFactory>();
            services.AddSingleton<ITemplateFactory<TemplateContainer, IContentTemplate>, IotCentralJsonPathContentTemplateFactory>();
            services.AddSingleton<ITemplateFactory<TemplateContainer, IContentTemplate>, CalculatedFunctionContentTemplateFactory>();
            services.AddSingleton<CollectionTemplateFactory<IContentTemplate, IContentTemplate>, CollectionContentTemplateFactory>();
        }

        public ITelemetryLogger AddApplicationInsightsLogging()
        {
            var instrumentationKey = Configuration.GetSection("APPINSIGHTS_INSTRUMENTATIONKEY").Value;

            TelemetryConfiguration telemetryConfig;
            TelemetryClient telemetryClient;

            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                telemetryConfig = new TelemetryConfiguration();
                telemetryClient = new TelemetryClient(telemetryConfig);
            }
            else
            {
                telemetryConfig = new TelemetryConfiguration()
                {
                    ConnectionString = $"InstrumentationKey={instrumentationKey}",
                };
                telemetryClient = new TelemetryClient(telemetryConfig);
            }

            var logger = new IomtTelemetryLogger(telemetryClient);
            return logger;
        }
    }
}
