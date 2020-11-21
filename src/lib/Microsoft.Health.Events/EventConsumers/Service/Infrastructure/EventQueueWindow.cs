// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Events.EventConsumers.Service.Infrastructure
{
    public class EventQueueWindow
    {
        private DateTime _windowEnd = DateTime.MinValue;
        private TimeSpan _flushTimespan;

        public EventQueueWindow(DateTime initDateTime, TimeSpan flushTimespan)
        {
            _windowEnd = initDateTime.Add(flushTimespan);
            _flushTimespan = flushTimespan;
        }

        public void IncrementWindow(DateTime currentEnqueudTime)
        {
            while (currentEnqueudTime >= _windowEnd)
            {
                _windowEnd = _windowEnd.Add(_flushTimespan);
            }
        }

        public DateTime GetWindowEnd()
        {
            return _windowEnd;
        }
    }
}
