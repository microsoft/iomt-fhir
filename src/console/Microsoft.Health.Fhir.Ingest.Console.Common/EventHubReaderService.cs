// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Events.EventHubProcessor;

namespace Microsoft.Health.Fhir.Ingest.Console
{
    public class EventHubReaderService : BackgroundService
    {
        private readonly IResumableEventProcessor _resumableEventProcessor;

        public EventHubReaderService(
            IResumableEventProcessor resumableEventProcessor)
        {
            _resumableEventProcessor = EnsureArg.IsNotNull(resumableEventProcessor, nameof(resumableEventProcessor));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _resumableEventProcessor.RunAsync(cancellationToken);
        }
    }
}
