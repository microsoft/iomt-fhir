// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Azure.Messaging.EventHubs;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest
{
    public static class EventDataTestHelper
    {
        public static EventData BuildEventFromJson(string json)
        {
            var token = JToken.Parse(json);

            var bodyContent = JToken.Parse(token["Body"].Value<string>());
            var propContent = JToken.Parse(token["Properties"].Value<string>());
            var syspContent = JToken.Parse(token["SystemProperties"].Value<string>());

            var properties = propContent.ToObject<Dictionary<string, object>>();
            var sysProperties = new MockEventSystemProperties(syspContent);

            var eventData = new MockEventData(
                eventBody: Convert.FromBase64String(bodyContent.Value<string>()),
                properties: properties,
                systemProperties: sysProperties);

            return eventData;
        }
    }
}
