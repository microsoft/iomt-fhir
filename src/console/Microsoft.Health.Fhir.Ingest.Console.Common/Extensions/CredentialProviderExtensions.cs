// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Auth;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class CredentialProviderExtensions
    {
        public static IServiceCollection AddDefaultCredentialProvider(this IServiceCollection services)
        {
            services.AddSingleton<IAzureCredentialProvider, AzureCredentialProvider>();
            return services;
        }
    }
}