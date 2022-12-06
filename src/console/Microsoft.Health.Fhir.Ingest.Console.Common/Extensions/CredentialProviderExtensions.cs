// --------------------------------------------------------------------------
// <copyright file="CredentialProviderExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Auth;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class CredentialProviderExtensions
    {
        public static void AddDefaultCredentialProvider(this IServiceCollection services)
        {
            services.AddSingleton<IAzureCredentialProvider, AzureCredentialProvider>();
        }
    }
}
