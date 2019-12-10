// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Text;
using EnsureThat;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class EventDataWithJsonBodyToJTokenConverter : IConverter<EventData, JToken>
    {
        public JToken Convert(EventData input)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            var body = input.Body.Count > 0
                ? JToken.Parse(Encoding.UTF8.GetString(input.Body.Array, input.Body.Offset, input.Body.Count))
                : null;
            var data = new { Body = body, input.Properties, input.SystemProperties };
            var token = JToken.FromObject(data);
            return token;
        }
    }
}
