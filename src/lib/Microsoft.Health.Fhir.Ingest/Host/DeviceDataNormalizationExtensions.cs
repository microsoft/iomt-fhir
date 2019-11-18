// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Health.Fhir.Ingest.Config;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    internal static class DeviceDataNormalizationExtensions
    {
        public static IWebJobsBuilder AddDeviceNormalization(this IWebJobsBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));

            builder.AddExtension<EventHubMeasurementCollectorProvider>()
                .BindOptions<EventHubMeasurementCollectorOptions>();

            return builder;
        }
    }
}