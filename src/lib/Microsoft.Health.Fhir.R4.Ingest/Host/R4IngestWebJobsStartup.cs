// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Health.Fhir.Ingest.Host;

[assembly: WebJobsStartup(typeof(R4IngestWebJobsStartup), "Fhir.R4.Ingest")]

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public class R4IngestWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddMeasurementFhirImport();
            builder.AddFhirHealthCheck();
        }
    }
}
