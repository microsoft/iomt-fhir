using EnsureThat;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Extensions.Fhir.Config;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Host;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;
using System;
using System.Linq;

namespace Microsoft.Health.Fhir.Ingest.Console.MeasurementCollectionToFhir
{
    public class ProcessorStartup
    {
        public ProcessorStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.GetSection("FhirService")
                .GetChildren()
                .ToList()
                .ForEach(env => Environment.SetEnvironmentVariable(env.Path, env.Value));

            services.Configure<ResourceIdentityOptions>(Configuration.GetSection("ResourceIdentity"));
            services.Configure<FhirClientFactoryOptions>(Configuration.GetSection("FhirClient"));

            services.AddFhirClient(Configuration);
            services.TryAddSingleton(ResolveResourceIdentityService);
            services.TryAddSingleton<IFhirService, FhirService>();

            services.TryAddSingleton<IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, Observation>, R4FhirLookupTemplateProcessor>();
            services.TryAddSingleton<IMemoryCache>(sp => new MemoryCache(Options.Create(new MemoryCacheOptions { SizeLimit = 5000 })));
            services.TryAddSingleton<FhirImportService, R4FhirImportService>();

            services.TryAddSingleton<ResourceManagementService>();
            services.TryAddSingleton<MeasurementFhirImportOptions>();
            services.TryAddSingleton<MeasurementFhirImportService>();
            services.TryAddSingleton(ResolveMeasurementImportProvider);
        }

        private MeasurementFhirImportProvider ResolveMeasurementImportProvider(IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            IOptions<MeasurementFhirImportOptions> options = Options.Create(new MeasurementFhirImportOptions());
            var logger = new LoggerFactory();
            var measurementImportService = new MeasurementFhirImportProvider(Configuration, options, logger, serviceProvider);

            return measurementImportService;
        }

        private static IResourceIdentityService ResolveResourceIdentityService(IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            var fhirService = serviceProvider.GetRequiredService<IFhirService>();
            var resourceIdentityOptions = serviceProvider.GetRequiredService<IOptions<ResourceIdentityOptions>>();
            return ResourceIdentityServiceFactory.Instance.Create(resourceIdentityOptions.Value, fhirService);
        }
    }
}
