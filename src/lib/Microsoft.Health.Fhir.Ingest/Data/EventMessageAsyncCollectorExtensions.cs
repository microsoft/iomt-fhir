// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Fhir.Ingest.Config;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public static class EventMessageAsyncCollectorExtensions
    {
        public static void AddEventMessageAsyncCollector(IServiceCollection services, IConfiguration configuration)
        {
            var collectorOptions = new CollectorOptions();
            configuration.GetSection(CollectorOptions.Settings).Bind(collectorOptions);

            if (collectorOptions.UseCompressionOnSend)
            {
                services.TryAddSingleton<IEnumerableAsyncCollector<IMeasurement>, MeasurementGroupToCompressedEventMessageAsyncCollector>();
            }
            else
            {
                services.TryAddSingleton<IEnumerableAsyncCollector<IMeasurement>, MeasurementToEventMessageAsyncCollector>();
            }
        }
    }
}
