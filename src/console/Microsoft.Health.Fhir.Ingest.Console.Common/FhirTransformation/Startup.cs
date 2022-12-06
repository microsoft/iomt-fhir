// --------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

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

            services.AddDefaultCredentialProvider();
            services.AddTemplateManager(Configuration);
            services.AddNormalizedEventReader(Configuration);
            services.AddNormalizedEventConsumer(Configuration);
            services.AddFhirImportServices(Configuration);
        }
    }
}
