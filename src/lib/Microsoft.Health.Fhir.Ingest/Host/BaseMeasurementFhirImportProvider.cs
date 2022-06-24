// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public abstract class BaseMeasurementFhirImportProvider : IExtensionConfigProvider
    {
        public BaseMeasurementFhirImportProvider(IConfiguration config, IOptions<MeasurementFhirImportOptions> options, ILoggerFactory loggerFactory, IExceptionTelemetryProcessor exceptionProcessor = null)
        {
            Config = EnsureArg.IsNotNull(config, nameof(config));
            Options = EnsureArg.IsNotNull(options, nameof(options));
            LoggerFactory = EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            ExceptionTelemetryProcessor = exceptionProcessor;
        }

        protected IConfiguration Config { get; }

        protected IOptions<MeasurementFhirImportOptions> Options { get; }

        protected ILoggerFactory LoggerFactory { get; }

        protected IExceptionTelemetryProcessor ExceptionTelemetryProcessor { get; }

        public void Initialize(ExtensionConfigContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            var fhirImportService = ResolveFhirImportService();
            var measurementFhirImportService = new MeasurementFhirImportService(fhirImportService, Options.Value, ExceptionTelemetryProcessor);

            context.AddBindingRule<MeasurementFhirImportAttribute>()
                .BindToInput(attr => measurementFhirImportService);
        }

        protected abstract FhirImportService ResolveFhirImportService();
    }
}
