// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Telemetry;

[assembly: FunctionsStartup(typeof(Microsoft.Health.Fhir.Ingest.Service.Startup))]

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));

            builder.Services.AddSingleton<ITelemetryLogger>(sp =>
            {
                var telemetryConfiguration = new TelemetryConfiguration();
                telemetryConfiguration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
                telemetryConfiguration.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY") ?? string.Empty;
                telemetryConfiguration.TelemetryProcessorChainBuilder.Build();
                TelemetryClient telemetryClient = new TelemetryClient(telemetryConfiguration);
                var telemetryLogger = new IomtTelemetryLogger(telemetryClient);
                return telemetryLogger;
            });
        }
    }
}