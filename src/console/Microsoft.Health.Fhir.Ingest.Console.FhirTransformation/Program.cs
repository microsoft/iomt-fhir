// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Fhir.Ingest.Console.Common.Extensions;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Ingest.Console.FhirTransformation
{
    public class Program
    {
        public static async Task Main()
        {
            var config = EnvironmentConfiguration.GetEnvironmentConfig();
            await CreateHostBuilder(config).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(IConfiguration config) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    Startup startup = new Startup(config);
                    startup.ConfigureServices(services);
                    services.AddApplicationInsightsLogging(config);
                    services.AddHostedService<EventHubReaderService>();
                });
    }
}
