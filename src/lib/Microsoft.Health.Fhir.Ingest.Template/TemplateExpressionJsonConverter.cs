// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
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
        public TemplateExpressionJsonConverter()
        {
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TemplateExpression) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new TemplateExpression()
                {
                    Value = (string)reader.Value,
                };
            }
            else
            {
                return JToken.Load(reader).ToObject<TemplateExpression>();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
