// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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
        }

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
