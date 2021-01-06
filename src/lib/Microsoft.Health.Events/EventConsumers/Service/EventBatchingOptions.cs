// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.EventConsumers.Service
{
    public class EventBatchingOptions
    {
        public const string Settings = "EventBatching";

        public int FlushTimespan { get; set; }

        public int MaxEvents { get; set; }
    }
}
