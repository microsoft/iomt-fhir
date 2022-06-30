// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Serialization
{
    public class LineNumberJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType != null && typeof(LineInfo).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Null)
            {
                var lineNumber = 0;
                var linePosition = 0;

                if (reader is IJsonLineInfo lineInfoReader && lineInfoReader != null)
                {
                    if (lineInfoReader.HasLineInfo())
                    {
                        lineNumber = lineInfoReader.LineNumber;
                        linePosition = lineInfoReader.LinePosition;
                    }
                }

                var lineInfoObject = Activator.CreateInstance(objectType) as LineInfo;
                var rawLineInfoObject = JObject.Load(reader, new JsonLoadSettings { LineInfoHandling = LineInfoHandling.Load });

                serializer.Populate(rawLineInfoObject.CreateReader(), lineInfoObject);

                lineInfoObject.LineNumber = lineNumber;
                lineInfoObject.LinePosition = linePosition;

                if (lineInfoObject is LineAwareJsonObject lineAwareJsonObject)
                {
                    lineAwareJsonObject.SetLineInfoProperties(rawLineInfoObject);
                }

                return lineInfoObject;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}