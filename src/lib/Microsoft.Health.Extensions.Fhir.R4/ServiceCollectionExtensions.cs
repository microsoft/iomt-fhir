// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Extensions.Host.Auth;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
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

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                });

            // Timeout policy for individual requests
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

            serviceCollection.AddHttpClient<IFhirClient, FhirClient>(client =>
            {
                client.BaseAddress = url;
                client.Timeout = TimeSpan.FromSeconds(60); // Overall timeout across all requests
            })
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(timeoutPolicy)
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
