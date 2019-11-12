// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Config;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Config
{
    public class MeasurementFhirImportOptions
    {
        public virtual ParallelTaskOptions ParallelTaskOptions { get; } = new ParallelTaskOptions { MaxConcurrency = 10 }; // TODO: Create configuration option in web config.

        public virtual ExceptionTelemetryProcessor ExceptionService { get; } = new ExceptionTelemetryProcessor();

        public virtual ITemplateFactory<string, ILookupTemplate<IFhirTemplate>> TemplateFactory { get; } = CollectionFhirTemplateFactory.Default;
    }
}
