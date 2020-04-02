// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Ingest.Service;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public abstract class BaseFhirHealthCheckProvider : IExtensionConfigProvider
    {
        public BaseFhirHealthCheckProvider(ILoggerFactory loggerFactory)
        {
            LoggerFactory = EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
        }

        protected IConfiguration Config { get; }

        protected ILoggerFactory LoggerFactory { get; }

        public void Initialize(ExtensionConfigContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            var fhirHealthService = ResolveFhirHealthService();
            var fhirHealthCheckService = new FhirHealthCheckService(fhirHealthService);

            context.AddBindingRule<FhirHealthCheckAttribute>()
                .BindToInput(attr => fhirHealthCheckService);
        }

        protected abstract FhirHealthService ResolveFhirHealthService();
    }
}
