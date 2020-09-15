// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Health.Common.EventHubs
{
    /// <summary>
    /// Setup an 'output' binding to an EventHub. This can be any output type compatible with an IAsyncCollector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class EventHubAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubAttribute"/> class.
        /// </summary>
        /// <param name="eventHubName">Name of the event hub as resolved against the <see cref="EventHubConfiguration"/> </param>
        public EventHubAttribute(string eventHubName)
        {
            EventHubName = eventHubName;
        }

        /// <summary>
        /// The name of the event hub. This is resolved against the <see cref="EventHubConfiguration"/>
        /// </summary>
        [AutoResolve]
        public string EventHubName { get; private set; }

        /// <summary>
        /// Gets or sets the optional connection string name that contains the Event Hub connection string. If missing, tries to use a registered event hub sender.
        /// </summary>
        [ConnectionString]
        public string Connection { get; set; }
    }
}