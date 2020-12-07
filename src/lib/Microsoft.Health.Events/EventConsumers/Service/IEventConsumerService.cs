// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.EventConsumers.Service
{
    public interface IEventConsumerService
    {
        Task ConsumeEvents(IEnumerable<IEventMessage> events);

        Task ConsumeEvent(IEventMessage eventArg);
    }
}
