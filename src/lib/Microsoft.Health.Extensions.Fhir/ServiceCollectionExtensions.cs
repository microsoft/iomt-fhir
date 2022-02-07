// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Extensions.Host.Auth;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class ServiceCollectionExtensions
    {
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
