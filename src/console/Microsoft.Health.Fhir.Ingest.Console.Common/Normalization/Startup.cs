﻿// --------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

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

            // Add default Azure credential provider
            services.AddDefaultCredentialProvider();

            // Add template manager
            services.AddTemplateManager(Configuration);

            // Add event processor
            services.AddEventProcessor(Configuration);

            // Add exception handling
            services.AddNormalizationExceptionTelemetryProcessor(Configuration);

            // Add normalization consumer services
            services.AddNormalizationEventConsumer(Configuration);

            // Add event processing meters
            services.AddEventProcessingMetricMeters();

            // Add event producer
            services.AddEventProducer(Configuration);
        }
    }
}
