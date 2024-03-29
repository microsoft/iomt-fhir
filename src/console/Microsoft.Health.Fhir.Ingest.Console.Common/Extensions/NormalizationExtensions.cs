﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Events.EventProducers;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Events.Telemetry;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class NormalizationExtensions
    {
        public static IServiceCollection AddEventProducer(this IServiceCollection services, IConfiguration config)
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

            return services;
        }

        public static IServiceCollection AddEventProcessor(this IServiceCollection services, IConfiguration config)
        {
            services.AddEventCheckpointing();

            services.AddSingleton((sp) =>
            {
                // Add EventHubClientOptions (Used for EventProcessorClient - common to all applications)
                var options = new EventHubClientOptions();
                config.GetSection("InputEventHub").Bind(options);
                var externalMiTokenProvider = sp.GetService<IAzureExternalIdentityCredentialProvider>();
                var serviceMiTokenProvider = sp.GetService<IAzureCredentialProvider>();
                var tokenProvider = externalMiTokenProvider ?? serviceMiTokenProvider;
                options.EventHubTokenCredential = tokenProvider?.GetCredential();
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

            return services;
        }

        public static IServiceCollection AddNormalizationEventConsumer(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<TemplateOptions>().Bind(config.GetSection(TemplateOptions.Settings));
            services.AddSingleton<Data.IConverter<IEventMessage, JObject>, EventMessageJObjectConverter>();
            services.AddSingleton<NormalizationEventConsumerService>();

            services.AddSingleton<IEnumerable<IEventConsumer>>((sp) =>
            {
                var consumer = sp.GetRequiredService<NormalizationEventConsumerService>();
                return new List<IEventConsumer>() { consumer };
            });

            return services;
        }

        public static IServiceCollection AddEventProcessingMetricMeters(this IServiceCollection services)
        {
            services.AddSingleton<IEventProcessingMetricMeters>((sp) =>
            {
                var processingMetric = EventMetrics.EventsConsumed(EventMetricDefinition.DeviceIngressSizeBytes);
                var meter = new IngressBytesEventProcessingMeter(processingMetric);
                var meters = new EventProcessingMetricMeters(new List<Events.Common.IEventProcessingMeter>() { meter });
                return meters;
            });

            return services;
        }
    }
}
