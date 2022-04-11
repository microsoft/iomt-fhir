// -------------------------------------------------------------------------------------------------	
// Copyright (c) Microsoft Corporation. All rights reserved.	
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.	
// -------------------------------------------------------------------------------------------------	

using System;
using System.Collections.Generic;
using Azure.Messaging.EventHubs;
using DevLab.JmesPath;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Storage;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Events.EventProducers;
using Microsoft.Health.Events.Repository;
using Microsoft.Health.Expressions;
using Microsoft.Health.Fhir.Ingest.Console.Template;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Common.Telemetry;
using IEventProcessingMeter = Microsoft.Health.Events.Common.IEventProcessingMeter;

namespace Microsoft.Health.Fhir.Ingest.Console
{
    public class Startup
    {
        private const string _normalizationAppType = ApplicationType.Normalization;
        private const string _measurementToFhirAppType = ApplicationType.MeasurementToFhir;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEventProcessorClientFactory, EventProcessorClientFactory>();
            services.AddSingleton<IEventProducerClientFactory, EventProducerClientFactory>();
            services.AddSingleton<BlobContainerClientFactory>();
            AddContentTemplateFactories(services);
            services.AddSingleton(ResolveTemplateManager);
            services.AddSingleton(ResolveEventConsumers);
            services.AddSingleton(ResolveCheckpointClient);
            services.AddSingleton(ResolveEventProcessingMetricMeters);
            services.AddSingleton(ResolveEventConsumerService);
            services.AddSingleton(ResolveEventProcessorClient);
            services.AddSingleton(ResolveEventProcessor);
        }
        public virtual TemplateManager ResolveTemplateManager(IServiceProvider serviceProvider)
        {
            var blobClientFactory = serviceProvider.GetRequiredService<BlobContainerClientFactory>();
            var containerOptions = new BlobContainerClientOptions();
            Configuration.GetSection("TemplateStorage").Bind(containerOptions);
            var containerClient = blobClientFactory.CreateStorageClient(containerOptions);
            var storageManager = new StorageManager(containerClient);
            var templateManager = new TemplateManager(storageManager);
            return templateManager;
        }

        public virtual List<IEventConsumer> ResolveEventConsumers(IServiceProvider serviceProvider)
        {
            var applicationType = GetConsoleApplicationType();

            string template;
            var eventConsumers = new List<IEventConsumer>();
            var templateManager = serviceProvider.GetRequiredService<TemplateManager>();
            var logger = serviceProvider.GetRequiredService<ITelemetryLogger>();

            if (applicationType == _normalizationAppType)
            {
                template = Configuration.GetSection("Template:DeviceContent").Value;
                var collector = ResolveEventCollector(serviceProvider);
                var collectionContentFactory = serviceProvider.GetRequiredService<CollectionTemplateFactory<IContentTemplate, IContentTemplate>>();
                var deviceDataNormalization = new Normalize.Processor(template, templateManager, collector, logger, collectionContentFactory);
                eventConsumers.Add(deviceDataNormalization);
            }
            else if (applicationType == _measurementToFhirAppType)
            {
                template = Configuration.GetSection("Template:FhirMapping").Value;
                var importService = serviceProvider.GetRequiredService<MeasurementFhirImportService>();
                var measurementCollectionToFhir = new MeasurementCollectionToFhir.Processor(template, templateManager, importService, logger);
                eventConsumers.Add(measurementCollectionToFhir);
            }
            else
            {
                throw new Exception($"Unable to determine template from application type {applicationType}");
            }

            return eventConsumers;
        }

        public virtual StorageCheckpointClient ResolveCheckpointClient(IServiceProvider serviceProvider)
        {
            var applicationType = GetConsoleApplicationType();

            var storageOptions = new StorageCheckpointOptions();
            Configuration.GetSection(StorageCheckpointOptions.Settings).Bind(storageOptions);
            var checkpointContainerOptions = new BlobContainerClientOptions();
            Configuration.GetSection("CheckpointStorage").Bind(checkpointContainerOptions);

            var factory = serviceProvider.GetRequiredService<BlobContainerClientFactory>();
            var checkpointBlobClient = factory.CreateStorageClient(checkpointContainerOptions);
            var logger = serviceProvider.GetRequiredService<ITelemetryLogger>();

            var eventProcessorOptions = new EventHubClientOptions();
            if (applicationType == _normalizationAppType)
            {
                Configuration.GetSection("InputEventHub").Bind(eventProcessorOptions);
            }
            else if (applicationType == _measurementToFhirAppType)
            {
                Configuration.GetSection("NormalizationEventHub").Bind(eventProcessorOptions);
            }

            storageOptions.BlobPrefix = $"{applicationType}/{storageOptions.BlobPrefix}";
            var checkpointClient = new StorageCheckpointClient(checkpointBlobClient, storageOptions, eventProcessorOptions, logger);
            return checkpointClient;
        }

        public virtual IEventProcessingMetricMeters ResolveEventProcessingMetricMeters(IServiceProvider serviceProvider)
        {
            var applicationType = GetConsoleApplicationType();

            if (applicationType == _normalizationAppType)
            {
                Metric processingMetric = EventMetrics.EventsConsumed(EventMetricDefinition.DeviceIngressSizeBytes);
                var meter = new Events.Common.EventProcessingMeter(processingMetric);
                var meters = new EventProcessingMetricMeters(new List<IEventProcessingMeter>() { meter });
                return meters;
            }

            return null;
        }

        public virtual IEventConsumerService ResolveEventConsumerService(IServiceProvider serviceProvider)
        {
            var eventConsumers = serviceProvider.GetRequiredService<List<IEventConsumer>>();
            var logger = serviceProvider.GetRequiredService<ITelemetryLogger>();
            return new EventConsumerService(eventConsumers, logger);
        }

        public virtual IEnumerableAsyncCollector<IMeasurement> ResolveEventCollector(IServiceProvider serviceProvider)
        {
            var eventHubProducerOptions = new EventHubClientOptions();
            Configuration.GetSection("NormalizationEventHub").Bind(eventHubProducerOptions);

            var eventHubProducerFactory = serviceProvider.GetRequiredService<IEventProducerClientFactory>();
            var eventHubProducerClient = eventHubProducerFactory.GetEventHubProducerClient(eventHubProducerOptions);
            var logger = serviceProvider.GetRequiredService<ITelemetryLogger>();

            return new MeasurementToEventMessageAsyncCollector(new EventHubProducerService(eventHubProducerClient), new HashCodeFactory(), logger);
        }

        public virtual EventProcessorClient ResolveEventProcessorClient(IServiceProvider serviceProvider)
        {
            var eventProcessorOptions = new EventHubClientOptions();
            var applicationType = GetConsoleApplicationType();

            if (applicationType == _normalizationAppType)
            {
                Configuration.GetSection("InputEventHub").Bind(eventProcessorOptions);
            }
            else if (applicationType == _measurementToFhirAppType)
            {
                Configuration.GetSection("NormalizationEventHub").Bind(eventProcessorOptions);
            }
            else
            {
                throw new Exception($"Unable to determine event processor options from application type {applicationType}");
            }

            var eventProcessorClientFactory = serviceProvider.GetRequiredService<IEventProcessorClientFactory>();
            var eventProcessorClientOptions = new EventProcessorClientOptions();
            eventProcessorClientOptions.MaximumWaitTime = TimeSpan.FromSeconds(60);

            var storageCheckpointClient = serviceProvider.GetRequiredService<StorageCheckpointClient>();
            var incomingEventReader = eventProcessorClientFactory.CreateProcessorClient(storageCheckpointClient.GetBlobContainerClient(), eventProcessorOptions, eventProcessorClientOptions);
            return incomingEventReader;
        }

        public virtual EventProcessor ResolveEventProcessor(IServiceProvider serviceProvider)
        {
            var eventConsumerService = serviceProvider.GetRequiredService<IEventConsumerService>();
            var eventProcessingMetricMeters = serviceProvider.GetService<IEventProcessingMetricMeters>();
            var checkpointClient = serviceProvider.GetRequiredService<StorageCheckpointClient>();
            var logger = serviceProvider.GetRequiredService<ITelemetryLogger>();
            var eventProcessorClient = serviceProvider.GetRequiredService<EventProcessorClient>();
            var eventBatchingOptions = new EventBatchingOptions();
            Configuration.GetSection(EventBatchingOptions.Settings).Bind(eventBatchingOptions);
            var eventBatchingService = new EventBatchingService(eventConsumerService, eventBatchingOptions, checkpointClient, logger, eventProcessingMetricMeters);
            var eventHubReader = new EventProcessor(eventProcessorClient, eventBatchingService, checkpointClient, logger);
            return eventHubReader;
        }

        public string GetConsoleApplicationType()
        {
            var applicationType = Environment.GetEnvironmentVariable("WEBJOBS_NAME");
            if (string.IsNullOrWhiteSpace(applicationType))
            {
                applicationType = Configuration.GetSection("Console:ApplicationType").Value;
            }

            return applicationType;
        }

        private void AddContentTemplateFactories(IServiceCollection services)
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
    }
}