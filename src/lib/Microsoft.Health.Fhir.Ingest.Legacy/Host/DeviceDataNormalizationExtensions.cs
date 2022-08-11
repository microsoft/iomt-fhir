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
    }
}