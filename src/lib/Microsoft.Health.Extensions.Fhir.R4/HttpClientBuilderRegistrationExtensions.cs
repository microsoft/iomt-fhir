// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Extensions.Host.Auth;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class HttpClientBuilderRegistrationExtensions
    {
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
                services.AddNamedManagedIdentityCredentialProvider();
                httpClientBuilder.AddHttpMessageHandler(x =>
                    ActivatorUtilities.CreateInstance<BearerTokenAuthorizationMessageHandler>(x, new object[] { uri, new ManagedIdentityAuthService() }));
            }
            else
            {
                services.AddNamedOAuth2ClientCredentialProvider();
                httpClientBuilder.AddHttpMessageHandler(x =>
                    ActivatorUtilities.CreateInstance<BearerTokenAuthorizationMessageHandler>(x, new object[] { uri, new OAuthConfidentialClientAuthService() }));
            }
        }
    }
}
