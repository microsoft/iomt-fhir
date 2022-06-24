// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Extensions.Host;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public static class DeviceDataNormalizationExtensions
    {
        public static IWebJobsBuilder AddDeviceNormalization(this IWebJobsBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));

            builder.AddExtension<EventHubMeasurementCollectorProvider>()
                .BindOptions<EventHubMeasurementCollectorOptions>();

            builder.AddExtension<DeviceDataNormalizationSettingsProvider>();

            builder.Services.AddDeviceNormalization();

            return builder;
        }

        public static IServiceCollection AddDeviceNormalization(this IServiceCollection serviceCollection)
        {
            EnsureArg.IsNotNull(serviceCollection, nameof(serviceCollection));

            IConfiguration config = serviceCollection.GetConfiguration();

            return serviceCollection.AddNormalizationExceptionTelemetryProcessor(config);
        }

        public static IServiceCollection AddNormalizationExceptionTelemetryProcessor(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            EnsureArg.IsNotNull(serviceCollection, nameof(serviceCollection));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            serviceCollection.Configure<NormalizationServiceOptions>(configuration.GetSection(NormalizationServiceOptions.Settings));

            var options = configuration.GetSection(NormalizationServiceOptions.Settings);

            if (options.GetValue<NormalizationErrorHandlingPolicy>("ErrorHandlingPolicy") == NormalizationErrorHandlingPolicy.DiscardMatchAndExtractErrors)
            {
                var processorConfig = new ExceptionTelemetryProcessorConfig() { HandledExceptionTypes = new Type[] { typeof(NormalizationDataMappingException) } };
                serviceCollection.AddSingleton<IExceptionTelemetryProcessorConfig>(processorConfig);
            }
            else
            {
                serviceCollection.AddSingleton<IExceptionTelemetryProcessorConfig>(new ExceptionTelemetryProcessorConfig());
            }

            serviceCollection.AddSingleton<NormalizationExceptionTelemetryProcessor>();

            return serviceCollection;
        }
    }
}