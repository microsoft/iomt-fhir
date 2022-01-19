// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Logging.Telemetry;
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

            var eventHubReader = serviceProvider.GetRequiredService<EventProcessor>();

            System.Console.WriteLine($"Reading from event hub type: {applicationType}");
            var ct = new CancellationToken();
            await eventHubReader.RunAsync(ct);
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
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ITelemetryLogger>(ConfigureLogging(config));

            if (applicationType == ApplicationType.Normalization)
            {
                Startup startup = new Startup(config);
                startup.ConfigureServices(serviceCollection);
                return serviceCollection;
            }
            else if (applicationType == ApplicationType.MeasurementToFhir)
            {
                MeasurementCollectionToFhir.ProcessorStartup measurementStartup = new MeasurementCollectionToFhir.ProcessorStartup(config);
                measurementStartup.ConfigureServices(serviceCollection);
                Startup startup = new Startup(config);
                startup.ConfigureServices(serviceCollection);
                return serviceCollection;
            }
            else
            {
                throw new Exception($"An invalid application type was provided: {applicationType}");
            }
        }

        public static ITelemetryLogger ConfigureLogging(IConfiguration configuration)
        {
            var instrumentationKey = configuration.GetSection("APPINSIGHTS_INSTRUMENTATIONKEY").Value;

            TelemetryConfiguration telemetryConfig;
            TelemetryClient telemetryClient;

            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                telemetryConfig = new TelemetryConfiguration();
                telemetryClient = new TelemetryClient(telemetryConfig);
            }
            else
            {
                telemetryConfig = new TelemetryConfiguration(instrumentationKey);
                telemetryClient = new TelemetryClient(telemetryConfig);
            }

            var logger = new IomtTelemetryLogger(telemetryClient);
            return logger;
        }
    }
}
