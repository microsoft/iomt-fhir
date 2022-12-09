// --------------------------------------------------------------------------
// <copyright file="TemplateExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

using DevLab.JmesPath;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Expressions;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class TemplateExtensions
    {
        public static IServiceCollection AddContentTemplateFactories(this IServiceCollection services)
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

            return services;
        }
    }
}
