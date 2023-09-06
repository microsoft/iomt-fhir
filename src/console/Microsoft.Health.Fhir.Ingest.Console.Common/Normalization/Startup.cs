// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Fhir.Ingest.Console.Common;
using Microsoft.Health.Fhir.Ingest.Console.Common.Extensions;
using Microsoft.Health.Fhir.Ingest.Host;

namespace Microsoft.Health.Fhir.Ingest.Console.Normalization
{
    public class Startup : StartupBase
    {
        public Startup(IConfiguration config)
            : base(config)
        {
        }

        public override string ApplicationType => Common.ApplicationType.Normalization;

        public override string OperationType => ConnectorOperation.Normalization;

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddDefaultCredentialProvider()
                .AddTemplateManager(Configuration)
                .AddEventProcessor(Configuration)
                .AddNormalizationExceptionTelemetryProcessor(Configuration)
                .AddNormalizationEventConsumer(Configuration)
                .AddEventProcessingMetricMeters()
                .AddEventProducerFactory()
                .AddEventProducer(Configuration);
        }
    }
}
