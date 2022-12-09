// --------------------------------------------------------------------------
// <copyright file="StartupBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Fhir.Ingest.Console.Common.Extensions;

namespace Microsoft.Health.Fhir.Ingest.Console.Common
{
    public abstract class StartupBase
    {
        protected StartupBase(IConfiguration config)
        {
            Configuration = config;
            ServiceCollection = new ServiceCollection();
        }

        public ServiceCollection ServiceCollection { get; set; }

        public IConfiguration Configuration { get; }

        public abstract string ApplicationType { get; }

        public abstract string OperationType { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddContentTemplateFactories()
                .AddEventProcessorClientFactory()
                .AddEventConsumerService()
                .AddEventHubConsumerClientFactory()
                .AddStorageClient(Configuration, ApplicationType)
                .AddResumableEventProcessor(Configuration);
        }
    }
}
