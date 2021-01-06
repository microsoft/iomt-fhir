using EnsureThat;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Console
{
    public class IomtLogger
    {
        public IomtLogger(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            var instrumentationKey = Configuration.GetSection("APPINSIGHTS_INSTRUMENTATIONKEY").Value;

            services.TryAddSingleton<ITelemetryLogger>(sp =>
            {
                var config = new TelemetryConfiguration(instrumentationKey);
                var telemetryClient = new TelemetryClient(config);
                return new IomtTelemetryLogger(telemetryClient);
            });
        }
    }
}
