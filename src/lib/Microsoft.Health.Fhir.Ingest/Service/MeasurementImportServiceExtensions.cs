// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Fhir.Ingest.Config;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementImportServiceExtensions
    {
        public static void AddImportService(IServiceCollection services, IConfiguration configuration)
        {
            var collectorOptions = new CollectorOptions();
            configuration.GetSection(CollectorOptions.Settings).Bind(collectorOptions);

            if (!collectorOptions.UseCompressionOnSend)
            {
                services.TryAddSingleton<IImportService, MeasurementFhirImportService>();
            }
            else
            {
                services.TryAddSingleton<IImportService, MeasurementGroupFhirImportService>();
            }
        }
    }
}