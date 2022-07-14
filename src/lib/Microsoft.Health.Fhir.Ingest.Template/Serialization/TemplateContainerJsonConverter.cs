// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Ingest.Template.Serialization.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Serialization
{
    /// <summary>
    /// A JsonConverter to create a TemplateContainer and preserve line details inside of the inner Template.
    ///
    /// Normal deserialization (i.e. JsonConvert.Deserialize) correctly creates a TemplateContainer but
    /// the inner 'Template' JToken does not preserve line numbers. This class manually creates the TemplateContainer
    /// and insures that line numbers are preserved.
    /// </summary>
    public class TemplateContainerJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(TemplateContainer) == objectType;
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

                var templateContainerObject = JObject.Load(reader);
                var innerTemplate = templateContainerObject.GetValue("template", StringComparison.InvariantCultureIgnoreCase);

                var templateContainer = new TemplateContainer();
                serializer.Populate(templateContainerObject.CreateReader(), templateContainer);
                /**
                 * At this point the TemplateContainer is fully populated but the inner 'Template' contains no line numbers.
                 * Replace the 'Template' property with that of the templateContainerObject, which will contain line
                 * information
                 */
                templateContainer.Template = innerTemplate;

                // Set line number details
                templateContainer.LineNumber = lineNumber;
                templateContainer.LinePosition = linePosition;
                templateContainer.SetLineInfoProperties(templateContainerObject);

                return templateContainer;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (value is TemplateContainer container)
            {
                JObject jObject = serializer.SerializeValue(container);

                jObject.WriteTo(writer);
                return;
            }

            throw new NotSupportedException($"TemplateContainerJsonConverter cannot convert type: {value.GetType()}");
        }
    }
}
