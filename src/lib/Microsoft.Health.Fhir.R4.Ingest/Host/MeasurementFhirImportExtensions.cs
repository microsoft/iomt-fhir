﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Health.Extensions.Fhir;
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

            // Register services
            builder.Services.AddSingleton<IFhirClient>(FhirClientFactory.Instance.Create());
            builder.Services.AddSingleton<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Observation>, R4FhirLookupTemplateProcessor>();
            builder.Services.AddSingleton<IResourceIdentityService>(
                sp =>
                {
                    var fhirClient = sp.GetRequiredService<IFhirClient>();
                    var resourceIdentityOptions = sp.GetRequiredService<IOptions<ResourceIdentityOptions>>();
                    return ResourceIdentityServiceFactory.Instance.Create(resourceIdentityOptions.Value, fhirClient);
                });
            builder.Services.AddSingleton<IMemoryCache>(sp => new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions { SizeLimit = 5000 })));
            builder.Services.AddSingleton<FhirImportService, R4FhirImportService>();

            // Register extensions
            builder.AddExtension<MeasurementFhirImportProvider>()
                .BindOptions<MeasurementFhirImportOptions>();

            return builder;
        }
    }
}
