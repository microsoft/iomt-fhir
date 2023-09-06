// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Fhir.Ingest.Console.Common;
using Microsoft.Health.Fhir.Ingest.Console.Common.Extensions;

namespace Microsoft.Health.Fhir.Ingest.Console.FhirTransformation
{
    public class Startup : StartupBase
    {
        public Startup(IConfiguration config)
            : base(config)
        {
        }

        public override string ApplicationType => Common.ApplicationType.MeasurementToFhir;

        public override string OperationType => ConnectorOperation.FHIRConversion;

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            Configuration.GetSection("FhirService")
                .GetChildren()
                .ToList()
                .ForEach(env => Environment.SetEnvironmentVariable(env.Path, env.Value));

            services.AddDefaultCredentialProvider()
                .AddTemplateManager(Configuration)
                .AddNormalizedEventReader(Configuration)
                .AddNormalizedEventConsumer(Configuration)
                .AddFhirImportServices(Configuration);
        }
    }
}
