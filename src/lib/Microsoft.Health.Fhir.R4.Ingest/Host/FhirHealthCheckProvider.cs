// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Ingest.Service;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public class FhirHealthCheckProvider : BaseFhirHealthCheckProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public FhirHealthCheckProvider(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
            : base(loggerFactory)
        {
            _serviceProvider = EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));
        }

        protected override FhirHealthService ResolveFhirHealthService() => _serviceProvider.GetRequiredService<FhirHealthService>();
    }
}
