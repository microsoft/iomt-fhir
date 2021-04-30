// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Events.EventHubProcessor;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Ingest.Console
{
    public class Program
    {
        public static async Task Main()
        {
            var config = GetEnvironmentConfig();
            var applicationType = GetConsoleApplicationType(config);

            var serviceCollection = GetRequiredServiceCollection(config, applicationType);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var incomingEventReader = serviceProvider.GetRequiredService<EventProcessorClient>();
            var eventHubReader = serviceProvider.GetRequiredService<EventProcessor>();

            System.Console.WriteLine($"Reading from event hub type: {applicationType}");
            var ct = new CancellationToken();
            await eventHubReader.RunAsync(incomingEventReader, ct);
        }

        public static string GetConsoleApplicationType(IConfiguration config)
        {
            var applicationType = Environment.GetEnvironmentVariable("WEBJOBS_NAME");
            if (string.IsNullOrWhiteSpace(applicationType))
            {
                applicationType = config.GetSection("Console:ApplicationType").Value;
            }

            return applicationType;
        }

        public static IConfiguration GetEnvironmentConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("local.appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        public static ServiceCollection GetRequiredServiceCollection(IConfiguration config, string applicationType)
        {
            if (applicationType == ApplicationType.Normalization)
            {
                var serviceCollection = new ServiceCollection();
                Startup startup = new Startup(config);
                startup.ConfigureServices(serviceCollection);
                return serviceCollection;
            }
            else if (applicationType == ApplicationType.MeasurementToFhir)
            {
                var serviceCollection = new ServiceCollection();
                MeasurementCollectionToFhir.ProcessorStartup measurementStartup = new MeasurementCollectionToFhir.ProcessorStartup(config);
                measurementStartup.ConfigureServices(serviceCollection);
                Startup startup = new Startup(config);
                startup.ConfigureServices(serviceCollection);
                return serviceCollection;
            }
            else
            {
                throw new Exception($"An invalid application type type was provided: {applicationType}");
            }
        }
    }
}
