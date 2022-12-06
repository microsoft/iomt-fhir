// --------------------------------------------------------------------------
// <copyright file="NormalizationExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Events.EventProducers;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class NormalizationExtensions
    {
        public static void AddEventProducer(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IEventHubMessageService, EventHubProducerService>();
            services.AddSingleton<IHashCodeFactory, HashCodeFactory>();
            EventMessageAsyncCollectorExtensions.AddEventMessageAsyncCollector(services, config);

            services.AddSingleton((sp) =>
            {
                var options = new EventHubClientOptions();
                config.GetSection("NormalizationEventHub").Bind(options);
                var tokenProvider = sp.GetService<IAzureCredentialProvider>();
                var eventHubProducerFactory = sp.GetRequiredService<IEventProducerClientFactory>();
                return eventHubProducerFactory.GetEventHubProducerClient(options, tokenProvider);
            });
        }

        public static void AddEventProcessor(this IServiceCollection services, IConfiguration config)
        {
            services.AddEventCheckpointing();

            services.AddSingleton((sp) =>
            {
                // Add EventHubClientOptions (Used for EventProcessorClient - common to all applications)
                var options = new EventHubClientOptions();
                config.GetSection("InputEventHub").Bind(options);
                return options;
            });
            services.AddSingleton((sp) =>
            {
                // Add EventProcessorClient
                var options = sp.GetRequiredService<EventHubClientOptions>();

                // Get IAzureExternalIdentityCredentialProvider if it exists, else use IAzureCredentialProvider
                var externalMiTokenProvider = sp.GetService<IAzureExternalIdentityCredentialProvider>();
                var serviceMiTokenProvider = sp.GetService<IAzureCredentialProvider>();
                var tokenProvider = externalMiTokenProvider ?? serviceMiTokenProvider;

                var eventProcessorClientFactory = sp.GetRequiredService<IEventProcessorClientFactory>();
                var eventProcessorClientOptions = new EventProcessorClientOptions() { MaximumWaitTime = TimeSpan.FromSeconds(60) };
                var storageCheckpointClient = sp.GetRequiredService<StorageCheckpointClient>();
                return eventProcessorClientFactory.CreateProcessorClient(storageCheckpointClient.GetBlobContainerClient(), options, eventProcessorClientOptions, tokenProvider);
            });
        }

        public static void AddNormalizationEventConsumer(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IEnumerable<IEventConsumer>>((sp) =>
            {
                // Add IEnumerable<IEventConsumer>() - (this is used by EventConsumerService)
                string template = config.GetSection("Template:DeviceContent").Value;
                var templateManager = sp.GetRequiredService<TemplateManager>();
                var logger = sp.GetRequiredService<ITelemetryLogger>();
                var collector = sp.GetRequiredService<IEnumerableAsyncCollector<IMeasurement>>();
                var collectionContentFactory = sp.GetRequiredService<CollectionTemplateFactory<IContentTemplate, IContentTemplate>>();
                var exceptionTelemetryProcessor = sp.GetRequiredService<NormalizationExceptionTelemetryProcessor>();
                return new List<IEventConsumer>() { new NormalizationEventConsumerService(new EventMessageJObjectConverter(), template, templateManager, collector, logger, collectionContentFactory, exceptionTelemetryProcessor) };
            });
        }

        public static void AddEventProcessingMetricMeters(this IServiceCollection services)
        {
            services.AddSingleton<IEventProcessingMetricMeters>((sp) =>
            {
                var processingMetric = EventMetrics.EventsConsumed(EventMetricDefinition.DeviceIngressSizeBytes);
                var meter = new IngressBytesEventProcessingMeter(processingMetric);
                var meters = new EventProcessingMetricMeters(new List<Events.Common.IEventProcessingMeter>() { meter });
                return meters;
            });
        }
    }
}
