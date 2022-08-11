// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Azure.EventHubs;
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

            var eventData = new EventData(Convert.FromBase64String(bodyContent.Value<string>()));
            var properties = propContent.ToObject<Dictionary<string, object>>();

            foreach (var p in properties)
            {
                eventData.Properties.Add(p);
            }

            eventData.SystemProperties = new EventData.SystemPropertiesCollection(0, default(DateTime), null, null);
            eventData.SystemProperties.Clear();
            var sysProperties = syspContent.ToObject<Dictionary<string, object>>();

            foreach (var sp in sysProperties)
            {
                eventData.SystemProperties.Add(sp.Key, sp.Value);
            }

            return eventData;
        }
    }
}
