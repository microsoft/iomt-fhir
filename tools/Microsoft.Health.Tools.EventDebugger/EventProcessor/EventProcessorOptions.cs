// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.Health.Tools.EventDebugger
{
    public class EventProcessorOptions
    {
        public TimeSpan EventReadTimeout { get; set; } = TimeSpan.FromSeconds(60);

        public int TotalEventsToProcess { get; set; } = 10;

        public DirectoryInfo OutputDirectory { get; set; }

        public DateTime EnqueuedTime { get; set; }
    }
}