// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Hl7.Fhir.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Config;
using Microsoft.Health.Extensions.Fhir.Repository;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    internal static class MeasurementFhirImportExtensions
    {
        public static IWebJobsBuilder AddMeasurementFhirImport(this IWebJobsBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));

            var serviceProvider = builder.Services.BuildServiceProvider();
            IConfiguration config = serviceProvider.GetService<IConfiguration>();

            Ensure.Any.IsNotNull(config, nameof(config));

            // Resolve configurations
            builder.Services.Configure<ResourceIdentityOptions>(config.GetSection("ResourceIdentity"));
            builder.Services.Configure<FhirClientFactoryOptions>(config.GetSection("FhirClient"));

            builder.Services.AddFhirClient(config);

            builder.Services.TryAddSingleton<IFhirServiceRepository, FhirServiceRepository>();

            builder.Services.TryAddSingleton<ResourceManagementService>();

            builder.Services.TryAddSingleton<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Observation>, R4FhirLookupTemplateProcessor>();
            builder.Services.TryAddSingleton<IMemoryCache>(sp => new MemoryCache(Options.Create(new MemoryCacheOptions { SizeLimit = 5000 })));
            builder.Services.TryAddSingleton<FhirImportService, R4FhirImportService>();

            builder.Services.TryAddSingleton<IResourceIdentityService, R4DeviceAndPatientCreateIdentityService>();
            builder.Services.TryAddSingleton<IResourceIdentityService, R4DeviceAndPatientLookupIdentityService>();
            builder.Services.TryAddSingleton<IResourceIdentityService, R4DeviceAndPatientWithEncounterLookupIdentityService>();
            builder.Services.TryAddSingleton<IFactory<IResourceIdentityService>, ResourceIdentityServiceFactory>();
            builder.Services.TryAddSingleton(sp => sp.GetRequiredService<IFactory<IResourceIdentityService>>().Create());

            // Register extensions
            builder.AddExtension<MeasurementFhirImportProvider>()
                .BindOptions<MeasurementFhirImportOptions>();

            return builder;
        }
    }
}
