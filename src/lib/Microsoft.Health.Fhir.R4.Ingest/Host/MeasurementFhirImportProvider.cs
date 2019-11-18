// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public class MeasurementFhirImportProvider : BaseMeasurementFhirImportProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public MeasurementFhirImportProvider(IConfiguration config, IOptions<MeasurementFhirImportOptions> options, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
            : base(config, options, loggerFactory)
        {
            _serviceProvider = EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));
        }

        protected override FhirImportService ResolveFhirImportService() => _serviceProvider.GetRequiredService<FhirImportService>();
    }
}
