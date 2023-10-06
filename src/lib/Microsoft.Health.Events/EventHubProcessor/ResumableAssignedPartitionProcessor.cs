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

        private CancellationTokenSource _cts = new CancellationTokenSource();

        public ResumableAssignedPartitionProcessor(PartitionLockingService lockingBackgroundService)
        {
            _lockingService = lockingBackgroundService;
        }

        public void Dispose()
        {
            // todo?
        }

        public async Task<bool> ResumeAsync(CancellationToken ct)
        {
            var isRunning = Interlocked.Read(ref _isRunning) == 1;
            await RunAsync(ct);
            return !isRunning;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            if (Interlocked.Exchange(ref _isRunning, 1) == 0)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                await _lockingService.StartAsync(_cts.Token);
            }
        }

        public Task SuspendAsync(CancellationToken ct)
        {
            if (Interlocked.Exchange(ref _isRunning, 0) == 1)
            {
                _cts.Cancel();
            }

            return Task.CompletedTask;
        }
    }
}
