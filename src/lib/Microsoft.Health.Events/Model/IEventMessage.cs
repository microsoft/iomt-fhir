// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Microsoft.Health.Events.Model
{
    public interface IEventMessage
    {
        string PartitionId { get; }

        ReadOnlyMemory<byte> Body { get; }

        long SequenceNumber { get; }

        long Offset { get; }

        DateTimeOffset EnqueuedTime { get; }

        IDictionary<string, object> Properties { get; }

        IReadOnlyDictionary<string, object> SystemProperties { get; }
    }
}
