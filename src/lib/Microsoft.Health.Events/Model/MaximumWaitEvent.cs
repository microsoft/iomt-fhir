// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Events.Model
{
    public class MaximumWaitEvent : EventMessage
    {
        public MaximumWaitEvent(string partitionId, DateTime dateTime)
            : base(partitionId, dateTime)
        {
        }
    }
}
