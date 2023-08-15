// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Extensions.Host.Auth;
using Microsoft.Health.Logging.Telemetry;
using FhirClient = Microsoft.Health.Fhir.Client.FhirClient;
using IFhirClient = Microsoft.Health.Fhir.Client.IFhirClient;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class FhirClientExtensions
    {
        public static IServiceCollection AddFhirClient(this IServiceCollection serviceCollection, IConfiguration configuration, IAzureCredentialProvider credentialProvider = null)
        {
            EnsureArg.IsNotNull(serviceCollection, nameof(serviceCollection));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            Uri url = new (configuration.GetValue<string>("FhirService:Url"));
            bool useManagedIdentity = configuration.GetValue<bool>("FhirClient:UseManagedIdentity");

            serviceCollection.TryAddSingleton<IFhirTokenProvider>(sp =>
            {
                var tokenProvider = sp.GetService<IAzureExternalIdentityCredentialProvider>() ?? sp.GetService<IAzureCredentialProvider>();

                if (useManagedIdentity)
                {
                    return new ManagedIdentityAuthService();
                }
                else if (tokenProvider != null)
                {
                    return new ManagedIdentityAuthService(tokenProvider);
                }
                else
                {
                    return new OAuthConfidentialClientAuthService();
                }
            });

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
            .AddAuthenticationHandler(url);

            return serviceCollection;
        }

        public static void AddAuthenticationHandler(
           this IHttpClientBuilder httpClientBuilder,
           Uri uri)
        {
            EnsureArg.IsNotNull(httpClientBuilder, nameof(httpClientBuilder));
            EnsureArg.IsNotNull(uri, nameof(uri));

            httpClientBuilder.AddHttpMessageHandler(sp =>
                    new BearerTokenAuthorizationMessageHandler(uri, sp.GetRequiredService<IFhirTokenProvider>().GetTokenCredential(), sp.GetRequiredService<ITelemetryLogger>()));
        }
    }
}
