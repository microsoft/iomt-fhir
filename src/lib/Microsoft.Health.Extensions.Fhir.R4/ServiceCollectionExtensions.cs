// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Extensions.Host.Auth;
using Microsoft.Health.Logging.Telemetry;
using FhirClient = Microsoft.Health.Fhir.Client.FhirClient;
using IFhirClient = Microsoft.Health.Fhir.Client.IFhirClient;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class ServiceCollectionExtensions
    {
        public static void AddFhirClient(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            EnsureArg.IsNotNull(serviceCollection, nameof(serviceCollection));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            var url = new Uri(configuration.GetValue<string>("FhirService:Url"));
            bool useManagedIdentity = configuration.GetValue<bool>("FhirClient:UseManagedIdentity");

            serviceCollection.AddSingleton(typeof(ITelemetryLogger), typeof(IomtTelemetryLogger));
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ITelemetryLogger>();

            serviceCollection.AddHttpClient<IFhirClient, FhirClient>(client =>
            {
                client.BaseAddress = url;
                client.Timeout = TimeSpan.FromSeconds(60);

                // Using discard because we don't need result
                var fhirClient = new FhirClient(client);
                _ = fhirClient.ValidateFhirClientAsync(logger);
                return fhirClient;
            })
            .AddAuthenticationHandler(serviceCollection, url, useManagedIdentity);
        }

        public static void AddNamedManagedIdentityCredentialProvider(this IServiceCollection serviceCollection)
        {
            EnsureArg.IsNotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.TryAddSingleton<ManagedIdentityAuthService>();
        }

        public static void AddNamedOAuth2ClientCredentialProvider(this IServiceCollection serviceCollection)
        {
            EnsureArg.IsNotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.TryAddSingleton<OAuthConfidentialClientAuthService>();
        }
    }
}
