// --------------------------------------------------------------------------
// <copyright file="FhirTransformationExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

using Azure.Messaging.EventHubs;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Console.FhirTransformation;
using Microsoft.Health.Fhir.Ingest.Host;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class FhirTransformationExtensions
    {
        public static IServiceCollection AddNormalizedEventReader(this IServiceCollection services, IConfiguration config)
        {
            services.AddEventCheckpointing();

            services.AddSingleton((sp) =>
            {
                // Add EventHubClientOptions (Used for EventProcessorClient - common to all applications)
                var options = new EventHubClientOptions();
                config.GetSection("NormalizationEventHub").Bind(options);
                return options;
            });
            services.AddSingleton((sp) =>
            {
                // Add EventProcessorClient
                var options = sp.GetRequiredService<EventHubClientOptions>();
                var tokenProvider = sp.GetService<IAzureCredentialProvider>();
                var eventProcessorClientFactory = sp.GetRequiredService<IEventProcessorClientFactory>();
                var eventProcessorClientOptions = new EventProcessorClientOptions() { MaximumWaitTime = TimeSpan.FromSeconds(60) };
                var storageCheckpointClient = sp.GetRequiredService<StorageCheckpointClient>();
                return eventProcessorClientFactory.CreateProcessorClient(storageCheckpointClient.GetBlobContainerClient(), options, eventProcessorClientOptions, tokenProvider);
            });

            return services;
        }

        public static IServiceCollection AddNormalizedEventConsumer(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<TemplateOptions>().Bind(config.GetSection(TemplateOptions.Settings));
            services.AddSingleton<Processor>();
            services.AddSingleton<IEnumerable<IEventConsumer>>((sp) =>
            {
                var processor = sp.GetRequiredService<Processor>();
                return new List<IEventConsumer>() { processor };
            });

            return services;
        }

        public static IServiceCollection AddFhirImportServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ResourceIdentityOptions>(config.GetSection("ResourceIdentity"));
            services.Configure<ObservationCacheOptions>(config.GetSection(ObservationCacheOptions.Settings));
            services.AddSingleton<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Observation>, R4FhirLookupTemplateProcessor>();
            services.AddSingleton<IFhirService, FhirService>();
            services.AddSingleton<FhirImportService, R4FhirImportService>();

            services.AddSingleton<IExceptionTelemetryProcessor, FhirExceptionTelemetryProcessor>();
            services.AddSingleton<MeasurementFhirImportOptions>();
            MeasurementImportServiceExtensions.AddImportService(services, config);

            // Add FHIR Client
            // Use IAzureExternalIdentityCredentialProvider if it exists, else use IAzureCredentialProvider
            var sp = services.BuildServiceProvider();
            var externalMiTokenProvider = sp.GetService<IAzureExternalIdentityCredentialProvider>();
            var serviceMiTokenProvider = sp.GetService<IAzureCredentialProvider>();
            var tokenProvider = externalMiTokenProvider ?? serviceMiTokenProvider;
            services.AddFhirClient(config, tokenProvider);

            services.AddSingleton<IMemoryCache>(sp => new MemoryCache(sp.GetRequiredService<IOptions<ObservationCacheOptions>>()));

            services.AddSingleton(sp =>
            {
                // Add MeasurementFhirImportProvider
                IOptions<MeasurementFhirImportOptions> options = Options.Create(new MeasurementFhirImportOptions());
                var logger = sp.GetRequiredService<ILoggerFactory>();
                return new MeasurementFhirImportProvider(config, options, logger, sp);
            });
            services.AddSingleton(sp =>
            {
                // Add IResourceIdentityService
                var fhirService = sp.GetRequiredService<IFhirService>();
                var resourceIdentityOptions = sp.GetRequiredService<IOptions<ResourceIdentityOptions>>();
                return ResourceIdentityServiceFactory.Instance.Create(resourceIdentityOptions.Value, fhirService);
            });

            return services;
        }
    }
}
