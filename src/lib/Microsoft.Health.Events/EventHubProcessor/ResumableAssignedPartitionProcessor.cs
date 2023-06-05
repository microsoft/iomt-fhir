// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class ResumableAssignedPartitionProcessor : IResumableEventProcessor
    {
        private PartitionLockingService _lockingService;

        private long _isRunning = 0;

        public ResumableAssignedPartitionProcessor(PartitionLockingService lockingBackgroundService)
        {
            _lockingService = lockingBackgroundService;
        }

        public void Dispose()
        {
            // todo?
        }

        public async Task ResumeAsync(CancellationToken ct)
        {
            if (Interlocked.Exchange(ref _isRunning, 1) == 0)
            {
                await _lockingService.StartAsync(ct);
            }
        }

        public async Task RunAsync(CancellationToken ct)
        {
            if (Interlocked.Exchange(ref _isRunning, 1) == 0)
            {
                await _lockingService.StartAsync(ct);
            }
        }

        public async Task SuspendAsync(CancellationToken ct)
        {
            if (Interlocked.Exchange(ref _isRunning, 0) == 1)
            {
                await _lockingService.StopAsync(ct);
            }
        }
    }
}
