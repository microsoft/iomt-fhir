// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Health.Common.EventHubs
{
    /// <summary>
    /// Setup an 'trigger' on a parameter to listen on events from an event hub.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class EventHubTriggerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubTriggerAttribute"/> class.
        /// </summary>
        /// <param name="eventHubName">Event hub to listen on for messages. </param>
        public EventHubTriggerAttribute(string eventHubName)
        {
            EventHubName = eventHubName;
        }

        /// <summary>
        /// Name of the event hub.
        /// </summary>
        public string EventHubName { get; private set; }

        /// <summary>
        /// Optional Name of the consumer group. If missing, then use the default name, "$Default"
        /// </summary>
        public string ConsumerGroup { get; set; }

        /// <summary>
        /// Gets or sets the optional app setting name that contains the Event Hub connection string. If missing, tries to use a registered event hub receiver.
        /// </summary>
        public string Connection { get; set; }
    }
}