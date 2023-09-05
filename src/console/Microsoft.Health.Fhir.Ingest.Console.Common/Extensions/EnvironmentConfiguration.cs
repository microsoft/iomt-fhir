// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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
