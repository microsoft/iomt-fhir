// --------------------------------------------------------------------------
// <copyright file="EnvironmentConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class EnvironmentConfiguration
    {
        public static IConfiguration GetEnvironmentConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("local.appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }
    }
}
