// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.EventHubs;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.Errors
{
    public interface IErrorMessageWithEvents : IErrorMessage
    {
        /// <summary>
        /// The message payload(s) that led to an error to be thrown.
        /// </summary>
        /// <remarks>
        /// This property provides the input message payload so the user can determine
        /// what message was correlated with the error.
        /// </remarks>
        IEnumerable<IEventMessage> RelatedLegacyEvents { get; set; }

        /// <summary>
        /// The message payload(s) that led to an error to be thrown.
        /// </summary>
        /// <remarks>
        /// This property provides the input message payload so the user can determine
        /// what message was correlated with the error.
        /// </remarks>
        IEnumerable<EventData> RelatedEvents { get; set; }
    }
}
