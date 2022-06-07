// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
using EnsureThat;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class EventDataWithJsonBodyToJTokenConverter : IConverter<EventData, JToken>
    {
        private const string BodyAttr = "Body";
        private const string PropertiesAttr = "Properties";
        private const string SystemPropertiesAttr = "SystemProperties";
        private readonly JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();

        public JToken Convert(EventData input)
        {
            EnsureArg.IsNotNull(input, nameof(input));
            JToken token = new JObject();
            JToken body = null;

            if (input.Body.Count > 0)
            {
                using (var stream = new MemoryStream(input.Body.Array))
                using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    body = JToken.ReadFrom(reader);
                }
            }

            token[BodyAttr] = body;
            token[PropertiesAttr] = JToken.FromObject(input.Properties, jsonSerializer);
            token[SystemPropertiesAttr] = JToken.FromObject(input.SystemProperties, jsonSerializer);

            return token;
        }
    }
}
