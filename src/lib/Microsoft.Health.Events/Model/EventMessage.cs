// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Health.Events.Model
{
    public class EventMessage : IEventMessage
    {
        public EventMessage(string partitionId, DateTime dateTime)
        {
            EnqueuedTime = dateTime;
            PartitionId = partitionId;
        }

        public EventMessage(
            string partitionId,
            ReadOnlyMemory<byte> body,
            long sequenceNumber,
            long offset,
            DateTimeOffset enqueuedTime,
            IDictionary<string, object> properties,
            IReadOnlyDictionary<string, object> systemProperties)
        {
            PartitionId = partitionId;
            Body = body;
            SequenceNumber = sequenceNumber;
            Offset = offset;
            EnqueuedTime = enqueuedTime;
            Properties = new Dictionary<string, object>(properties);
            SystemProperties = systemProperties == null ? new Dictionary<string, object>() : systemProperties.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public string PartitionId { get; }

        public ReadOnlyMemory<byte> Body { get; }

        public long SequenceNumber { get; }

        public long Offset { get; }

        public DateTimeOffset EnqueuedTime { get; }

        public IDictionary<string, object> Properties { get; }

        public IReadOnlyDictionary<string, object> SystemProperties { get; }
    }
}
