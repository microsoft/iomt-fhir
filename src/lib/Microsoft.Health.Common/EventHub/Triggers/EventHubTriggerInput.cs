// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace Microsoft.Health.Common.EventHubs
{
    // The core object we get when an EventHub is triggered.
    // This gets converted to the user type (EventData, string, poco, etc)
    internal sealed class EventHubTriggerInput
    {
        // If != -1, then only process a single event in this batch.
        private int _selector = -1;

        internal EventData[] Events { get; set; }

        internal PartitionContext PartitionContext { get; set; }

        public bool IsSingleDispatch
        {
            get
            {
                return _selector != -1;
            }
        }

        public static EventHubTriggerInput New(EventData eventData)
        {
            return new EventHubTriggerInput
            {
                PartitionContext = null,
                Events = new EventData[]
                {
                      eventData,
                },
                _selector = 0,
            };
        }

        public EventHubTriggerInput GetSingleEventTriggerInput(int idx)
        {
            return new EventHubTriggerInput
            {
                Events = Events,
                PartitionContext = PartitionContext,
                _selector = idx,
            };
        }

        public EventData GetSingleEventData()
        {
            return Events[_selector];
        }

        public Dictionary<string, string> GetTriggerDetails(PartitionContext context)
        {
            if (Events.Length == 0)
            {
                return new Dictionary<string, string>();
            }

            string offset, enqueueTimeUtc, sequenceNumber;
            if (IsSingleDispatch)
            {
                offset = Events[0].SystemProperties?.Offset;
                enqueueTimeUtc = Events[0].SystemProperties?.EnqueuedTimeUtc.ToString("o");
                sequenceNumber = Events[0].SystemProperties?.SequenceNumber.ToString();
            }
            else
            {
                EventData first = Events[0];
                EventData last = Events[Events.Length - 1];

                offset = $"{first.SystemProperties?.Offset}-{last.SystemProperties?.Offset}";
                enqueueTimeUtc = $"{first.SystemProperties?.EnqueuedTimeUtc.ToString("o")}-{last.SystemProperties?.EnqueuedTimeUtc.ToString("o")}";
                sequenceNumber = $"{first.SystemProperties?.SequenceNumber}-{last.SystemProperties?.SequenceNumber}";
            }

            return new Dictionary<string, string>()
            {
                { "PartionId", context.PartitionId },
                { "Offset", offset },
                { "EnqueueTimeUtc", enqueueTimeUtc },
                { "SequenceNumber", sequenceNumber },
                { "Count", Events.Length.ToString() },
            };
        }
    }
}