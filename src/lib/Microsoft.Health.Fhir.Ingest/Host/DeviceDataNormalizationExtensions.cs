// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common.Errors;
using Microsoft.Health.Extensions.Host;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;

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
            serviceCollection.AddSingleton(TelemetryProcessorFactory);

            return serviceCollection;
        }

        private static NormalizationExceptionTelemetryProcessor TelemetryProcessorFactory(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<NormalizationServiceOptions>>();

            var errorMessageService = serviceProvider.GetService<IErrorMessageService>();

            if (errorMessageService == null)
            {
                return options.Value.ErrorHandlingPolicy switch
                {
                    NormalizationErrorHandlingPolicy.DiscardMatchAndExtractErrors => new NormalizationExceptionTelemetryProcessor(typeof(NormalizationDataMappingException)),
                    _ => new NormalizationExceptionTelemetryProcessor(),
                };
            }
            else
            {
                return options.Value.ErrorHandlingPolicy switch
                {
                    NormalizationErrorHandlingPolicy.DiscardMatchAndExtractErrors => new NormalizationExceptionTelemetryProcessor(errorMessageService, typeof(NormalizationDataMappingException)),
                    _ => new NormalizationExceptionTelemetryProcessor(errorMessageService),
                };
            }
        }
    }
}