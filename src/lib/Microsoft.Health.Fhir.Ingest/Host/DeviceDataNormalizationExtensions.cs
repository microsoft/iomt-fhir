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
using Microsoft.Health.Extensions.Host;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    internal static class DeviceDataNormalizationExtensions
    {
        public static IWebJobsBuilder AddDeviceNormalization(this IWebJobsBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));

            builder.AddExtension<EventHubMeasurementCollectorProvider>()
                .BindOptions<EventHubMeasurementCollectorOptions>();

            IConfiguration config = builder.GetConfiguration();

            builder.Services.Configure<NormalizationServiceOptions>(config.GetSection(NormalizationServiceOptions.Settings));
            builder.Services.AddSingleton(TelemetryProcessorFactory);
            builder.AddExtension<DeviceDataNormalizationSettingsProvider>();

            return builder;
        }

        private static NormalizationExceptionTelemetryProcessor TelemetryProcessorFactory(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<NormalizationServiceOptions>>();

            return options.Value.ErrorHandlingPolicy switch
            {
                NormalizationErrorHandlingPolicy.DiscardMatchAndExtractErrors => new NormalizationExceptionTelemetryProcessor(typeof(NormalizationDataMappingException)),
                _ => new NormalizationExceptionTelemetryProcessor(),
            };
        }
    }
}