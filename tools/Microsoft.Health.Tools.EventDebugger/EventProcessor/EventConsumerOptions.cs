// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public class EventConsumerOptions
    {
        public string ConnectionString { get; set; }

        public string ConsumerGroup { get; set; }
    }
}