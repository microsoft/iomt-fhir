// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Config;
using Microsoft.Health.Extensions.Fhir.Repository;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;
using FhirClient = Microsoft.Health.Fhir.Client.FhirClient;
using IFhirClient = Microsoft.Health.Fhir.Client.IFhirClient;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    internal static class FhirHealthCheckExtensions
    {
        internal static IWebJobsBuilder AddFhirHealthCheck(this IWebJobsBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));

            var serviceProvider = builder.Services.BuildServiceProvider();
            IConfiguration config = serviceProvider.GetService<IConfiguration>();

            Ensure.Any.IsNotNull(config, nameof(config));

            // Resolve configurations
            builder.Services.Configure<ResourceIdentityOptions>(config.GetSection("ResourceIdentity"));
            builder.Services.Configure<FhirClientFactoryOptions>(config.GetSection("FhirClient"));

            // Register services
            var url = new Uri(Environment.GetEnvironmentVariable("FhirService:Url"));
            bool useManagedIdentity = config.GetValue<bool>("FhirClient:UseManagedIdentity");
            builder.Services.AddHttpClient<IFhirClient, FhirClient>(sp =>
            {
                sp.BaseAddress = url;
            }).AddAuthenticationHandler(builder.Services, url, useManagedIdentity);

            builder.Services.TryAddSingleton<IFhirServiceRepository, FhirServiceRepository>();

            builder.Services.TryAddSingleton<ResourceManagementService>();

            builder.Services.TryAddSingleton<FhirHealthService, R4FhirHealthService>();

            builder.AddExtension<FhirHealthCheckProvider>();
            return builder;
        }
    }
}
