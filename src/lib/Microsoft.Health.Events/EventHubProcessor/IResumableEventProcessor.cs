// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public interface IResumableEventProcessor : IDisposable
    {
        Task ResumeAsync(CancellationToken ct);

        Task RunAsync(CancellationToken ct);

        Task SuspendAsync(CancellationToken ct);
    }
}