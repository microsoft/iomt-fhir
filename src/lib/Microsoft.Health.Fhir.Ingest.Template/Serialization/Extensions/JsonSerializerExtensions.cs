// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Health.Fhir.Ingest.Template.Serialization.Extensions
{
    public static class JsonSerializerExtensions
    {
        public static JObject SerializeValue<TValue>(this JsonSerializer serializer, TValue value)
            where TValue : class
        {
            if (value == null)
            {
                return null;
            }

            JObject jObject = new JObject();
            JsonObjectContract contract = serializer.ContractResolver.ResolveContract(value.GetType()) as JsonObjectContract;

            PropertyInfo[] propertyInfos = typeof(TValue).GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                JsonProperty jsonProperty = contract.Properties.GetProperty(propertyInfo.Name, StringComparison.OrdinalIgnoreCase);

                if (jsonProperty != null)
                {
                    string propertyName = jsonProperty.PropertyName;
                    object propertyValue = jsonProperty.ValueProvider.GetValue(value);

                    if (propertyValue != null)
                    {
                        if (propertyValue is JToken jTokenValue)
                        {
                            jObject[propertyName] = jTokenValue;
                        }
                        else
                        {
                            jObject[propertyName] = JToken.FromObject(propertyValue, serializer);
                        }
                    }
                }
            }

            return jObject;
        }
    }
}
