// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using EnsureThat;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));

            var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

            if (instrumentationKey != null)
            {
                var configDescriptor = builder.Services.SingleOrDefault(tc => tc.ServiceType == typeof(TelemetryConfiguration));
                if (configDescriptor?.ImplementationFactory == null)
                {
                    throw new Exception($"Unable to retrieve TelemetryConfiguration of type {typeof(TelemetryConfiguration)}");
                }

                var implFactory = configDescriptor.ImplementationFactory;

                builder.Services.Remove(configDescriptor);
                builder.Services.AddSingleton<ITelemetryLogger>(provider =>
                {
                    if (!(implFactory.Invoke(provider) is TelemetryConfiguration config))
                    {
                        throw new Exception("Unable to build TelemetryConfiguration");
                    }

                    config.TelemetryProcessorChainBuilder.Build();

                    var telemetryClient = new TelemetryClient(config);
                    return new IomtTelemetryLogger(telemetryClient);
                });
            }
            else
            {
                builder.Services.AddSingleton<ITelemetryLogger>(sp =>
                {
                    var config = new TelemetryConfiguration();
                    var telemetryClient = new TelemetryClient(config);
                    return new IomtTelemetryLogger(telemetryClient);
                });
            }
        }
    }
}