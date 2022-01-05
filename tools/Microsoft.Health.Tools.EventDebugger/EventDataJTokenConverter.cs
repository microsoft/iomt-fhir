// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Text;
using Azure.Messaging.EventHubs;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class EventDataJTokenConverter : IConverter<EventData, JToken>
    {
        public JToken Convert(EventData input)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            var body = JToken.Parse(Encoding.UTF8.GetString(input.EventBody));
            var data = new { Body = body, input.Properties, input.SystemProperties };
            var token = JToken.FromObject(data);
            return token;
        }
    }
}
