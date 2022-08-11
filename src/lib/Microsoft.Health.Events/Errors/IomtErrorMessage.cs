// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Azure.Messaging.EventHubs;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.Errors
{
    public class IomtErrorMessage : IErrorMessageWithEvents
    {
        public IomtErrorMessage(Exception ex, IDictionary<string, object> dictionary = null)
        {
            Type = ex.GetType().Name;
            Details = ex.Message;
            ErrorTimestamp = DateTimeOffset.UtcNow;

            RelatedLegacyEvents = ex.GetRelatedLegacyEvents();
            RelatedEvents = ex.GetRelatedEvents();

            Values = dictionary;
        }

        /// <summary>
        /// A unique identifier for the ErrorMessage
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The type of error.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A string describing the error that occurred.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// The time when the error occurred.
        /// </summary>
        public DateTimeOffset ErrorTimestamp { get; set; }

        /// <summary>
        /// The message payload(s) that led to an error to be thrown.
        /// </summary>
        /// <remarks>
        /// This property provides the input message payload so the user can determine
        /// what message was correlated with the error.
        /// </remarks>
        public IEnumerable<IEventMessage> RelatedLegacyEvents { get; set; }

        /// <summary>
        /// The message payload(s) that led to an error to be thrown.
        /// </summary>
        /// <remarks>
        /// This property provides the input message payload so the user can determine
        /// what message was correlated with the error.
        /// </remarks>
        public IEnumerable<EventData> RelatedEvents { get; set; }

        /// <summary>
        /// Optional properties that provide additional context for the Error
        /// </summary>
        public IDictionary<string, object> Values { get; }
    }
}
