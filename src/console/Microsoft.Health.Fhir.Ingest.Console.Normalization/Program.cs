// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Fhir.Ingest.Console.Common.Extensions;

namespace Microsoft.Health.Fhir.Ingest.Console.Normalization
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .Build()
                .RunAsync();
        }

        /// <summary>
        /// DefaultBuilder will load IConfiguration from several sources including environment variables and appsettings.
        /// See documentation for complete details.
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.host.createdefaultbuilder?view=dotnet-plat-ext-6.0"/>
        /// Once upgraded to .NET 7 can switch to CreateApplicationBuilder.
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.host.createapplicationbuilder?view=dotnet-plat-ext-7.0"/>
        /// </summary>
        /// <param name="args">Arguments to configure the host builder.</param>
        /// <returns>Initialized <see cref="IHostBuilder" /></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddLocalAppSettings();
            })
            .ConfigureServices((hostContext, services) =>
            {
                IConfiguration config = hostContext.Configuration;
                Startup startup = new (config);
                startup.ConfigureServices(services);
                services.AddApplicationInsightsLogging(config);
                services.AddHostedService<EventHubReaderService>();
            });
    }
}
