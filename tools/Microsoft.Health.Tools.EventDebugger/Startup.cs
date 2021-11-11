
using System;
using Azure.Messaging.EventHubs.Consumer;
using DevLab.JmesPath;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Expressions;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Health.Tools.EventDebugger.EventProcessor;
using Microsoft.Health.Tools.EventDebugger.Extensions;
using Microsoft.Health.Tools.EventDebugger.TemplateLoader;


namespace Microsoft.Health.Tools.EventDebugger
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = EnsureArg.IsNotNull(configuration, nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<EventProcessorOptions>(_configuration.GetSection(EventProcessorOptions.Category));
            services.Configure<EventConsumerOptions>(_configuration.GetSection(EventConsumerOptions.Category));
            
            services.AddSingleton(_configuration);
            services.AddSingleton<ITelemetryLogger, SimpleTelemetryLogger>();
            services.AddSingleton<IConversionResultWriter, LocalConversionResultWriter>();
            AddContentTemplateFactories(services);
            services.AddSingleton<ITemplateLoader>(sp => 
            {
                var deviceTemplatePath = _configuration.GetArgument("DeviceTemplatePath", true);
                var contentFactory = sp.GetRequiredService<CollectionTemplateFactory<IContentTemplate, IContentTemplate>>();
                return new DeviceTemplateLoader(deviceTemplatePath, contentFactory);
            });
            services.AddSingleton(sp => 
            {
                var eventConsumerClient = new DeviceEventProcessor(
                    sp.GetRequiredService<ILogger<DeviceEventProcessor>>(),
                    new EventDataJTokenConverter(),
                    sp.GetRequiredService<ITemplateLoader>(),
                    sp.GetRequiredService<IConversionResultWriter>());
                return eventConsumerClient;
            });
        }

        private void AddContentTemplateFactories(IServiceCollection services)
        {
            services.AddSingleton<IExpressionRegister>(sp => new AssemblyExpressionRegister(typeof(IExpressionRegister).Assembly, sp.GetRequiredService<ITelemetryLogger>()));
            services.AddSingleton(
                sp =>
                {
                    var jmesPath = new JmesPath();
                    var expressionRegister = sp.GetRequiredService<IExpressionRegister>();
                    expressionRegister.RegisterExpressions(jmesPath.FunctionRepository);
                    return jmesPath;
                });
            services.AddSingleton<IExpressionEvaluatorFactory, TemplateExpressionEvaluatorFactory>();
            services.AddSingleton<ITemplateFactory<TemplateContainer, IContentTemplate>, JsonPathContentTemplateFactory>();
            services.AddSingleton<ITemplateFactory<TemplateContainer, IContentTemplate>, IotJsonPathContentTemplateFactory>();
            services.AddSingleton<ITemplateFactory<TemplateContainer, IContentTemplate>, IotCentralJsonPathContentTemplateFactory>();
            services.AddSingleton<ITemplateFactory<TemplateContainer, IContentTemplate>, CalculatedFunctionContentTemplateFactory>();
            services.AddSingleton<CollectionTemplateFactory<IContentTemplate, IContentTemplate>, CollectionContentTemplateFactory>();
        }
    }
}