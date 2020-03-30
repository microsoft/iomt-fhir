// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Common;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Config;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;

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
            builder.Services.TryAddSingleton<IFactory<IFhirClient>, FhirClientFactory>();
            builder.Services.TryAddSingleton<IFhirClient>(sp => sp.GetRequiredService<IFactory<IFhirClient>>().Create());
            builder.Services.TryAddSingleton<FhirHealthService, R4FhirHealthService>();

            builder.AddExtension<FhirHealthCheckProvider>();
            return builder;
        }
    }
}
