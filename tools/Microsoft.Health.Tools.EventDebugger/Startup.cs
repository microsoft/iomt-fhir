
using System;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Microsoft.Health.Tools.EventDebugger.EventProcessor;
using Microsoft.Health.Tools.EventDebugger.TemplateLoader;
using Microsoft.Health.Expressions;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using DevLab.JmesPath;

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
            services.AddSingleton(_configuration);
            services.AddSingleton<ITelemetryLogger, SimpleTelemetryLogger>();
            AddContentTemplateFactories(services);
            services.AddSingleton<ITemplateLoader>(sp => 
            {
                var deviceTemplatePath = GetArgument("DeviceTemplatePath", true);
                var contentFactory = sp.GetRequiredService<CollectionTemplateFactory<IContentTemplate, IContentTemplate>>();
                return new DeviceTemplateLoader(deviceTemplatePath, contentFactory);
            });
            services.AddSingleton(sp => 
            {
                var connectionString = GetArgument("ConnectionString", true);
                var consumerGroup = GetArgument("ConsumerGroup", true);
                var eventConsumerClient = new EventHubConsumerClient(consumerGroup, connectionString);
                return eventConsumerClient;
            });
            services.AddSingleton(sp => 
            {
                var connectionString = GetArgument("ConnectionString", true);
                var consumerGroup = GetArgument("ConsumerGroup", true);
                var eventConsumerClient = new DeviceEventProcessor(
                    sp.GetRequiredService<EventHubConsumerClient>(),
                    sp.GetRequiredService<ILogger<DeviceEventProcessor>>(),
                    new EventDataJTokenConverter(),
                    sp.GetRequiredService<ITemplateLoader>(),
                    TimeSpan.FromMinutes(10));
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

        private string GetArgument(string key, bool required = false)
        {
            var value = _configuration[key];
            if (required && string.IsNullOrWhiteSpace(value)) 
            {
                throw new ArgumentException($"Missing value for configuration item {key}");
            }
            return value;
        }
    }
}