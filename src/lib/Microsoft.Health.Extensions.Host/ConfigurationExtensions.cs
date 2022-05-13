// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Health.Extensions.Host
{
    public static class ConfigurationExtensions
    {
        public static IConfiguration GetConfiguration(this IWebJobsBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));
            return builder.Services.GetConfiguration();
        }

        public static IConfiguration GetConfiguration(this IServiceCollection serviceCollection)
        {
            EnsureArg.IsNotNull(serviceCollection, nameof(serviceCollection));
            var serviceProvider = serviceCollection.BuildServiceProvider();
            IConfiguration config = serviceProvider.GetService<IConfiguration>();

            return config;
        }
    }
}
