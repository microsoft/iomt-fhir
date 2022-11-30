// --------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Messaging.EventHubs;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Common.Storage;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Common;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Extensions.Fhir.Config;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Client;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Console.Common;
using Microsoft.Health.Fhir.Ingest.Host;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Console.FhirTransformation
{
    public class Startup : StartupBase
    {
        public Startup()
            : base()
        {
        }

        public override string ApplicationType => Common.ApplicationType.MeasurementToFhir;

        public override string OperationType => ConnectorOperation.FHIRConversion;

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            Configuration.GetSection("FhirService")
                .GetChildren()
                .ToList()
                .ForEach(env => Environment.SetEnvironmentVariable(env.Path, env.Value));

            // for open source, use default azure credential
            services.AddSingleton<IAzureCredentialProvider, AzureCredentialProvider>();

            services.Configure<ResourceIdentityOptions>(Configuration.GetSection("ResourceIdentity"));
            services.Configure<ObservationCacheOptions>(Configuration.GetSection(ObservationCacheOptions.Settings));
            services.AddSingleton<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Observation>, R4FhirLookupTemplateProcessor>();
            services.AddSingleton<IFactory<IFhirClient>, FhirClientFactory>();
            services.AddSingleton<IFhirService, FhirService>();
            services.AddSingleton<FhirImportService, R4FhirImportService>();
            services.AddSingleton<IExceptionTelemetryProcessor, FhirExceptionTelemetryProcessor>();
            services.AddSingleton<MeasurementFhirImportOptions>();
            MeasurementImportServiceExtensions.AddImportService(services, Configuration);
            services.AddSingleton(sp => sp.GetRequiredService<IFactory<IFhirClient>>().Create());

            services.AddSingleton(sp => Options.Create(new FhirClientFactoryOptions { UseManagedIdentity = true }));

            services.AddSingleton(sp => {
                // Get IAzureExternalIdentityCredentialProvider if it exists, else use IAzureCredentialProvider
                var externalMiTokenProvider = sp.GetService<IAzureExternalIdentityCredentialProvider>();
                var serviceMiTokenProvider = sp.GetService<IAzureCredentialProvider>();
                var tokenProvider = externalMiTokenProvider ?? serviceMiTokenProvider;
                return Options.Create(new FhirClientFactoryOptions { CredentialProvider = tokenProvider });
            });

            // services.AddSingleton<IHealthChecker, FhirServiceHealthChecker>();
            services.AddSingleton<IMemoryCache>(sp => new MemoryCache(sp.GetRequiredService<IOptions<ObservationCacheOptions>>()));
            services.AddSingleton((sp) =>
            {
                // Add EventHubClientOptions (Used for EventProcessorClient - common to all applications)
                var options = new EventHubClientOptions();
                Configuration.GetSection("NormalizationEventHub").Bind(options);
                return options;
            });
            services.AddSingleton(sp =>
            {
                // Add MeasurementFhirImportProvider
                IOptions<MeasurementFhirImportOptions> options = Options.Create(new MeasurementFhirImportOptions());
                var logger = sp.GetRequiredService<ILoggerFactory>();
                return new MeasurementFhirImportProvider(Configuration, options, logger, sp);
            });
            services.AddSingleton(sp =>
            {
                // Add IResourceIdentityService
                var fhirService = sp.GetRequiredService<IFhirService>();
                var resourceIdentityOptions = sp.GetRequiredService<IOptions<ResourceIdentityOptions>>();
                return ResourceIdentityServiceFactory.Instance.Create(resourceIdentityOptions.Value, fhirService);
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
            services.AddSingleton((sp) =>
            {
                // Add TemplateManager
                var tokenProvider = sp.GetService<IAzureCredentialProvider>();
                var blobClientFactory = sp.GetRequiredService<BlobContainerClientFactory>();
                var containerOptions = new BlobContainerClientOptions();
                Configuration.GetSection("TemplateStorage").Bind(containerOptions);
                var containerClient = blobClientFactory.CreateStorageClient(containerOptions, tokenProvider);
                return new TemplateManager(containerClient);
            });
            services.AddSingleton<IEnumerable<IEventConsumer>>((sp) =>
            {
                // Add IEnumerable<IEventConsumer>() - (this is used by EventConsumerService)
                string template = Configuration.GetSection("Template:FhirMapping").Value;
                var templateManager = sp.GetRequiredService<TemplateManager>();
                var logger = sp.GetRequiredService<ITelemetryLogger>();
                var importService = sp.GetRequiredService<IImportService>();
                return new List<IEventConsumer>() { new Processor(template, templateManager, importService, logger) };
            });
        }
    }
}
