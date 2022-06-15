// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Config;
using Microsoft.Health.Common.Errors;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Config
{
    public class MeasurementFhirImportOptions
    {
        public MeasurementFhirImportOptions()
        {
            ParallelTaskOptions = new ParallelTaskOptions { MaxConcurrency = 10 };
            TemplateFactory = CollectionFhirTemplateFactory.Default;
            ExceptionService = new FhirExceptionTelemetryProcessor();
        }

        public MeasurementFhirImportOptions(IErrorMessageService errorMessageService)
        {
            ParallelTaskOptions = new ParallelTaskOptions { MaxConcurrency = 10 };
            TemplateFactory = CollectionFhirTemplateFactory.Default;
            ExceptionService = new FhirExceptionTelemetryProcessor(errorMessageService);
        }

        public virtual ParallelTaskOptions ParallelTaskOptions { get; }

        public virtual IExceptionTelemetryProcessor ExceptionService { get; }

        public virtual ITemplateFactory<string, ITemplateContext<ILookupTemplate<IFhirTemplate>>> TemplateFactory { get; }
    }
}
