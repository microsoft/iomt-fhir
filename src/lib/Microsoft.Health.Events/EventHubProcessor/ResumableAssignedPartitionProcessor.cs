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
        private PartitionLockingBackgroundService _lockingBackgroundService;

        private bool _isRunning = false;

        public ResumableAssignedPartitionProcessor(PartitionLockingBackgroundService lockingBackgroundService)
        {
            _lockingBackgroundService = lockingBackgroundService;
        }

        public void Dispose()
        {
            // todo?
        }

        public async Task ResumeAsync(CancellationToken ct)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                await _lockingBackgroundService.StartAsync(ct);
            }
        }

        public async Task RunAsync(CancellationToken ct)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                await _lockingBackgroundService.StartAsync(ct);
            }
        }

        public async Task SuspendAsync(CancellationToken ct)
        {
            await _lockingBackgroundService.StopAsync(ct);
            _isRunning = false;
        }
    }
}
