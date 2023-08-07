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
        public static IConfigurationBuilder AddLocalAppSettings(this IConfigurationBuilder builder, bool reloadOnChange = true)
        {
            return builder.AddJsonFile("local.appsettings.json", true, reloadOnChange);
        }
    }
}
