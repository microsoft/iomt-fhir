// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Azure.Messaging.EventHubs;

namespace Microsoft.Health.Fhir.Ingest
{
    internal class MockEventData : EventData
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="MockEventData"/> class.
        /// </summary>
        ///
        /// <param name="eventBody">The raw data to use as the body of the event.</param>
        /// <param name="properties">The set of free-form event properties to send with the event.</param>
        /// <param name="systemProperties">The set of system properties received from the Event Hubs service.</param>
        /// <param name="sequenceNumber">The sequence number assigned to the event when it was enqueued in the associated Event Hub partition.</param>
        /// <param name="offset">The offset of the event when it was received from the associated Event Hub partition.</param>
        /// <param name="enqueuedTime">The date and time, in UTC, of when the event was enqueued in the Event Hub partition.</param>
        /// <param name="partitionKey">The partition hashing key applied to the batch that the associated <see cref="EventData"/>, was sent with.</param>
        ///
        public MockEventData(
            ReadOnlyMemory<byte> eventBody,
            IDictionary<string, object> properties = null,
            IReadOnlyDictionary<string, object> systemProperties = null,
            long sequenceNumber = long.MinValue,
            long offset = long.MinValue,
            DateTimeOffset enqueuedTime = default,
            string partitionKey = null)
            : base(eventBody, properties, systemProperties, sequenceNumber, offset, enqueuedTime, partitionKey)
        {
        }
    }
}
