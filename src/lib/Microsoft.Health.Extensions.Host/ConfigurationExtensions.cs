// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Health.Extensions.Host
{
    public static class ConfigurationExtensions
    {
        public static IConfiguration GetConfiguration(this IWebJobsBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            IConfiguration config = serviceProvider.GetService<IConfiguration>();

            return config;
        }
    }
}
