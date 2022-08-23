// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Template.Serialization.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Health.Fhir.Ingest.Template.Serialization
{
    public class AbstractJsonConverter : JsonConverter
    {
        private readonly Type _baseType;
        private readonly string _typePropertyName;
        private readonly string _typePropertyNameCamelCase;
        private readonly IDictionary<string, Type> _typeLookup;
        private readonly JsonSerializerSettings _knownTypeSettings;

        public AbstractJsonConverter(Type baseType, string typePropertyName)
        {
            EnsureArg.IsNotNull(baseType, nameof(baseType));
            EnsureArg.IsNotNullOrWhiteSpace(typePropertyName, nameof(typePropertyName));

            _baseType = baseType;
            _typePropertyName = typePropertyName;
            _typePropertyNameCamelCase = char.ToLowerInvariant(typePropertyName[0]) + typePropertyName.Substring(1);

            _typeLookup = _baseType.Assembly.GetTypes()
                .Where(t => _baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .ToDictionary(t => TrimBaseType(t.Name, _baseType.Name), StringComparer.InvariantCultureIgnoreCase);

            _knownTypeSettings = new JsonSerializerSettings
            {
                ContractResolver = new KnownTypeContractResolver(baseType),
                TypeNameHandling = TypeNameHandling.None,
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == _baseType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var typeNameToken = token[_typePropertyName] ?? token[_typePropertyNameCamelCase];

            if (typeNameToken == null)
            {
                throw new NotImplementedException($"Property {_typePropertyName} is missing.");
            }

            var typeName = typeNameToken.Value<string>();

            if (!string.IsNullOrEmpty(typeName) && _typeLookup.TryGetValue(typeName, out var targetType))
            {
                // Converter attribute is inherited to children classes, call deserialize with special settings to avoid infinite recursion.
                return JsonConvert.DeserializeObject(token.ToString(), targetType, _knownTypeSettings);
            }

            throw new NotImplementedException(typeName);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (value is FhirValueType fhirValue)
            {
                if (_typeLookup.TryGetValue(fhirValue.ValueType, out Type type))
                {
                    JObject jObject = serializer.SerializeValue(fhirValue, type);
                    jObject.WriteTo(writer);

                    return;
                }
            }

            throw new NotSupportedException($"AbstractJsonConverter cannot convert type: {value.GetType()}");
        }

        private static string TrimBaseType(string typeName, string baseTypeName)
        {
            if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(baseTypeName) || typeName.Length <= baseTypeName.Length)
            {
                return typeName;
            }

            if (typeName.Substring(typeName.Length - baseTypeName.Length) == baseTypeName)
            {
                return typeName.Substring(0, typeName.Length - baseTypeName.Length);
            }

            return typeName;
        }

        private class KnownTypeContractResolver : DefaultContractResolver
        {
            private readonly Type _baseType;

            public KnownTypeContractResolver(Type baseType)
            {
                EnsureArg.IsNotNull(baseType, nameof(baseType));
                _baseType = baseType;
            }

            protected override JsonConverter ResolveContractConverter(Type objectType)
            {
                if (_baseType.IsAssignableFrom(objectType) && !objectType.IsAbstract)
                {
                    return null;
                }

                return base.ResolveContractConverter(objectType);
            }
        }
    }
}
