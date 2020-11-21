// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Events.Model
{
    public class Event
    {
        public Event(string partitionId, DateTime dateTime)
        {
            EnqueuedTime = dateTime;
            PartitionId = partitionId;
        }

        public Event(
            string partitionId,
            ReadOnlyMemory<byte> body,
            long sequenceNumber,
            long offset,
            DateTimeOffset enqueuedTime,
            IReadOnlyDictionary<string, object> systemProperties)
        {
            PartitionId = partitionId;
            Body = body;
            SequenceNumber = sequenceNumber;
            Offset = offset;
            EnqueuedTime = enqueuedTime;
            SystemProperties = new Dictionary<string, object>(systemProperties);
        }

        public string PartitionId { get; }

        public ReadOnlyMemory<byte> Body { get; }

        public long SequenceNumber { get; }

        public long Offset { get; }

        public DateTimeOffset EnqueuedTime { get; }

        public Dictionary<string, object> SystemProperties { get; }
    }
}
