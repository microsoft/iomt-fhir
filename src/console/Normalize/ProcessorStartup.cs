// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Fhir.Ingest.Config;

namespace Microsoft.Health.Fhir.Ingest.Console.Normalize
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
            var outputEventHubConnection = Configuration.GetSection("OutputEventHub").Value;
            var outputEventHubName = outputEventHubConnection.Substring(outputEventHubConnection.LastIndexOf('=') + 1);

            EnsureArg.IsNotNullOrEmpty(outputEventHubConnection);
            EnsureArg.IsNotNullOrEmpty(outputEventHubName);

            services.Configure<EventHubMeasurementCollectorOptions>(options =>
            {
                options.AddSender(outputEventHubName, outputEventHubConnection);
            });
        }
    }
}
