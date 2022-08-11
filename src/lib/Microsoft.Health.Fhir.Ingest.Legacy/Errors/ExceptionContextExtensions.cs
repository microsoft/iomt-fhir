// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Health.Events.Errors
{
    public static class ExceptionContextExtensions
    {
        private const string EventsAttr = "RelatedEvents";

        public static void AddEventContext(this Exception ex, IEnumerable<EventData> events)
        {
            ex.Data[EventsAttr] = events;
        }
    }
}
