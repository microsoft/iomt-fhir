// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
using EnsureThat;
using Microsoft.Health.Events.Model;
using Microsoft.Toolkit.HighPerformance;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class EventMessageJTokenConverter : IConverter<IEventMessage, JToken>
    {
        public JToken Convert(IEventMessage input)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            JToken body = null;

            if (input.Body.Length > 0)
            {
                using StreamReader streamReader = new StreamReader(input.Body.AsStream(), Encoding.UTF8);
                using JsonReader jsonReader = new JsonTextReader(streamReader);
                body = JToken.ReadFrom(jsonReader);
            }

            var data = new { Body = body, input.Properties, input.SystemProperties };
            var token = JToken.FromObject(data);
            return token;
        }
    }
}
