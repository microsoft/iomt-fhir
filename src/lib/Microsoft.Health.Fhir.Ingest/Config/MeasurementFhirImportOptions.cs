// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Config;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Config
{
    public class MeasurementFhirImportOptions
    {
        public MeasurementFhirImportOptions()
        {
            ParallelTaskOptions = new ParallelTaskOptions { MaxConcurrency = 10 };
            TemplateFactory = CollectionFhirTemplateFactory.Default;
        }

        public virtual ParallelTaskOptions ParallelTaskOptions { get; set; }

        public virtual ITemplateFactory<string, ITemplateContext<ILookupTemplate<IFhirTemplate>>> TemplateFactory { get; set; }
    }
}
