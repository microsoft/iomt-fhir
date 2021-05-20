// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Health.Fhir.Ingest.Host;

[assembly: WebJobsStartup(typeof(IngestWebJobsStartup), "Fhir.Ingest")]

namespace Microsoft.Health.Fhir.Ingest.Host
{
    public class IngestWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDeviceNormalization();
            builder.AddDeviceIngressLogging();
        }
    }
}
