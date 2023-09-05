// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddApplicationInsightsLogging(this IServiceCollection services, IConfiguration configuration)
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
                telemetryConfig = new TelemetryConfiguration()
                {
                    ConnectionString = $"InstrumentationKey={instrumentationKey}",
                };
                telemetryClient = new TelemetryClient(telemetryConfig);
            }

            var logger = new IomtTelemetryLogger(telemetryClient);
            services.AddSingleton<ITelemetryLogger>(logger);

            return services;
        }
    }
}
