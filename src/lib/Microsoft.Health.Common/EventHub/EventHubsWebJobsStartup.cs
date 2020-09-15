// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Common.EventHubs;

[assembly: WebJobsStartup(typeof(EventHubsWebJobsStartup))]

namespace Microsoft.Health.Common.EventHubs
{
    public class EventHubsWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddEventHubs();
        }
    }
}
