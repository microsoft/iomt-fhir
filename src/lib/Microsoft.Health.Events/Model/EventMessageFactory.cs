// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.EventHubs.Processor;

namespace Microsoft.Health.Events.Model
{
    public static class EventMessageFactory
    {
        public static IEventMessage CreateEvent(ProcessEventArgs eventArgs)
        {
            var eventMessage = new EventMessage(
                eventArgs.Partition.PartitionId,
                eventArgs.Data.Body,
                eventArgs.Data.Offset,
                eventArgs.Data.SequenceNumber,
                eventArgs.Data.EnqueuedTime.UtcDateTime,
                eventArgs.Data.Properties,
                eventArgs.Data.SystemProperties);

            return eventMessage;
        }
    }
}
