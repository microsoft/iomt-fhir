// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Events.EventHubProcessor;
using Microsoft.Health.Logging.Telemetry;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Ingest.Console.Normalization
{
    public class Program
    {
        public static async Task Main()
        {
            var startup = new Startup();
            startup.ConfigureServices(startup.ServiceCollection);
            startup.ServiceCollection.AddSingleton<ITelemetryLogger>(startup.AddApplicationInsightsLogging());
            var serviceProvider = startup.ServiceCollection.BuildServiceProvider();

            var eventHubReader = serviceProvider.GetRequiredService<IResumableEventProcessor>();

            var ct = new CancellationToken();
            await eventHubReader.RunAsync(ct);
        }
    }
}
