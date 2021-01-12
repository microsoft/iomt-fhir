﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Config;
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

            builder.Services.TryAddSingleton<IFactory<FhirClient>, FhirClientFactory>();
            builder.Services.TryAddSingleton(sp => sp.GetRequiredService<IFactory<FhirClient>>().Create());
            builder.Services.TryAddSingleton<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Observation>, R4FhirLookupTemplateProcessor>();
            builder.Services.TryAddSingleton(ResolveResourceIdentityService);
            builder.Services.TryAddSingleton<IMemoryCache>(sp => new MemoryCache(Options.Create(new MemoryCacheOptions { SizeLimit = 5000 })));
            builder.Services.TryAddSingleton<FhirImportService, R4FhirImportService>();

            // Register extensions
            builder.AddExtension<MeasurementFhirImportProvider>()
                .BindOptions<MeasurementFhirImportOptions>();

            return builder;
        }

        private static IResourceIdentityService ResolveResourceIdentityService(IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            var fhirClient = serviceProvider.GetRequiredService<FhirClient>();
            var resourceIdentityOptions = serviceProvider.GetRequiredService<IOptions<ResourceIdentityOptions>>();
            return ResourceIdentityServiceFactory.Instance.Create(resourceIdentityOptions.Value, fhirClient);
        }
    }
}
