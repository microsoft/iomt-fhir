// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public abstract class BaseMeasurementFhirImportProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration _config;
        private readonly IOptions<MeasurementFhirImportOptions> _options;
        private readonly ILoggerFactory _loggerFactory;

        public BaseMeasurementFhirImportProvider(IConfiguration config, IOptions<MeasurementFhirImportOptions> options, ILoggerFactory loggerFactory)
        {
            _config = EnsureArg.IsNotNull(config, nameof(config));
            _options = EnsureArg.IsNotNull(options, nameof(options));
            _loggerFactory = EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
        }

        protected IConfiguration Config => _config;

        protected IOptions<MeasurementFhirImportOptions> Options => _options;

        protected ILoggerFactory LoggerFactory => _loggerFactory;

        public void Initialize(ExtensionConfigContext context)
        {
            var logger = _loggerFactory.CreateLogger(LogCategories.Executor);

            var fhirImportService = ResolveFhirImportService();
            var measurementFhirImportService = new MeasurementFhirImportService(fhirImportService, Options.Value, logger);

            context.AddBindingRule<MeasurementFhirImportAttribute>()
                .BindToInput(attr => measurementFhirImportService);
        }

        protected abstract FhirImportService ResolveFhirImportService();
    }
}
