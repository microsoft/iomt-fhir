// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
using EnsureThat;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Toolkit.HighPerformance;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class EventMessageJObjectConverter : IConverter<IEventMessage, JObject>
    {
        private const string BodyAttr = "Body";
        private const string PropertiesAttr = "Properties";
        private const string SystemPropertiesAttr = "SystemProperties";

        private readonly JsonLoadSettings loadSettings = new () { LineInfoHandling = LineInfoHandling.Ignore };

        private readonly JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();

        public JObject Convert(IEventMessage input)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            JObject token = new ();
            JToken body = null;

            try
            {
                if (input.Body.Length > 0)
                {
                    using StreamReader streamReader = new StreamReader(input.Body.AsStream(), Encoding.UTF8);
                    using JsonReader jsonReader = new JsonTextReader(streamReader);
                    body = JToken.ReadFrom(jsonReader, loadSettings);
                }

                token[BodyAttr] = body;
                token[PropertiesAttr] = JToken.FromObject(input.Properties, jsonSerializer);
                token[SystemPropertiesAttr] = JToken.FromObject(input.SystemProperties, jsonSerializer);
            }
            catch (JsonReaderException ex)
            {
                throw new InvalidDataFormatException("Invalid event message. Cannot be parsed into a JSON object.", ex);
            }

            return token;
        }
    }
}
