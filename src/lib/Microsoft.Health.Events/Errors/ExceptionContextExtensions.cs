﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Azure.EventHubs;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.Errors
{
    public static class ExceptionContextExtensions
    {
        private const string EventsAttr = "RelatedEvents";

        // remove when we move off of IEventMessage
        public static void AddEventContext(this Exception ex, IEnumerable<IEventMessage> events)
        {
            ex.Data[EventsAttr] = events;
        }

        public static void AddEventContext(this Exception ex, IEventMessage evt)
        {
            var list = new List<IEventMessage>() { evt };
            ex.Data[EventsAttr] = list;
        }

        // remove when we move off of IEventMessage
        public static IEnumerable<IEventMessage> GetRelatedLegacyEvents(this Exception ex)
        {
            IEnumerable<IEventMessage> events = ex.Data[EventsAttr] as IEnumerable<IEventMessage>;

            if (events != null)
            {
                return events;
            }
            else
            {
                return new List<IEventMessage>();
            }
        }

        // consider upgrading to Azure.Messaging.EventHubs
        public static void AddEventContext(this Exception ex, IEnumerable<EventData> events)
        {
            ex.Data[EventsAttr] = events;
        }

        public static IEnumerable<EventData> GetRelatedEvents(this Exception ex)
        {
            IEnumerable<EventData> events = ex.Data[EventsAttr] as IEnumerable<EventData>;

            if (events != null)
            {
                return events;
            }
            else
            {
                return new List<EventData>();
            }
        }
    }
}
