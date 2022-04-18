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
    public static class FhirClientExtensions
    {
        public static IServiceCollection AddFhirClient(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            EnsureArg.IsNotNull(serviceCollection, nameof(serviceCollection));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            var url = new Uri(configuration.GetValue<string>("FhirService:Url"));
            bool useManagedIdentity = configuration.GetValue<bool>("FhirClient:UseManagedIdentity");

            serviceCollection.AddHttpClient<IFhirClient, FhirClient>((client, sp) =>
            {
                client.BaseAddress = url;
                client.Timeout = TimeSpan.FromSeconds(60);

                var logger = sp.GetRequiredService<ITelemetryLogger>();

                // Using discard because we don't need result
                var fhirClient = new FhirClient(client);
                _ = fhirClient.ValidateFhirClientAsync(logger);

                return fhirClient;
            })
            .AddAuthenticationHandler(serviceCollection, url, useManagedIdentity);

            return serviceCollection;
        }

        public static void AddAuthenticationHandler(
           this IHttpClientBuilder httpClientBuilder,
           IServiceCollection services,
           Uri uri,
           bool useManagedIdentity)
        {
            EnsureArg.IsNotNull(httpClientBuilder, nameof(httpClientBuilder));
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(uri, nameof(uri));

            if (useManagedIdentity)
            {
                services.TryAddSingleton(new ManagedIdentityAuthService());
                httpClientBuilder.AddHttpMessageHandler(sp =>
                    new BearerTokenAuthorizationMessageHandler(uri, sp.GetRequiredService<ManagedIdentityAuthService>(), sp.GetRequiredService<ITelemetryLogger>()));
            }
            else
            {
                services.TryAddSingleton(new OAuthConfidentialClientAuthService());
                httpClientBuilder.AddHttpMessageHandler(sp =>
                    new BearerTokenAuthorizationMessageHandler(uri, sp.GetRequiredService<OAuthConfidentialClientAuthService>(), sp.GetRequiredService<ITelemetryLogger>()));
            }
        }
    }
}
