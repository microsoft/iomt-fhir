// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Template.Serialization.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Serialization
{
    /// <summary>
    /// A custom JsonConverter which supports creating a TemplateExpression from either a String or JObject.
    ///
    /// Example:
    ///
    /// {
    /// "typeMatchExpression" : "@.heartRate"
    /// }
    ///
    /// or
    /// {
    ///     "typeMatchExpression" : {
    ///         "value" : "@.heartRate",
    ///         "language" : "JsonPath"
    ///     }
    /// }
    /// </summary>
    public class TemplateExpressionJsonConverter : JsonConverter
    {
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>() { new LineNumberJsonConverter() },
        };

        public override bool CanConvert(Type objectType)
        {
            return typeof(TemplateExpression) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                var templateExpression = new TemplateExpression()
                {
                    Value = (string)reader.Value,
                };

                if (reader is IJsonLineInfo lineInfoReader && lineInfoReader != null && lineInfoReader.HasLineInfo())
                {
                    templateExpression.LineInfoForProperties = new Dictionary<string, LineInfo>();
                    templateExpression.LineNumber = lineInfoReader.LineNumber;
                    templateExpression.LinePosition = lineInfoReader.LinePosition;
                    templateExpression.LineInfoForProperties[nameof(templateExpression.Value)] =
                        new LineInfo()
                        {
                            LineNumber = lineInfoReader.LineNumber,
                            LinePosition = lineInfoReader.LinePosition,
                        };
                }

                return templateExpression;
            }
            else
            {
                return JToken.Load(reader).ToObject<TemplateExpression>(JsonSerializer.Create(jsonSerializerSettings));
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }

            if (value is TemplateExpression expression)
            {
                if (expression.Language == null)
                {
                    // The Language property is null so this expression is the default language.
                    writer.WriteValue(expression.Value);
                    return;
                }

                JObject json = serializer.SerializeValue(expression);

                json.WriteTo(writer);
                return;
            }

            throw new NotSupportedException($"TemplateExpressionJsonConverter cannot convert type: {value.GetType()}");
        }
    }
}
