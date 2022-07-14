// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Health.Fhir.Ingest.Template.Serialization
{
    public class LineAwareContractResolver : IContractResolver
    {
        private readonly IContractResolver _defaultContractResolver;

        public LineAwareContractResolver(IContractResolver defaultContractResolver, bool includeLineInfo = false)
        {
            _defaultContractResolver = EnsureArg.IsNotNull(defaultContractResolver, nameof(defaultContractResolver));
            IncludeLineInfo = includeLineInfo;
        }

        public bool IncludeLineInfo { get; set; }

        public JsonContract ResolveContract(Type type)
        {
            if (!IncludeLineInfo && type.IsSubclassOf(typeof(LineInfo)))
            {
                JsonObjectContract contract = _defaultContractResolver.ResolveContract(type) as JsonObjectContract;

                // 'contract' is a shared object and we lock here to prevent race conditions.
                lock (contract)
                {
                    RemoveProperty(nameof(LineAwareJsonObject.LineInfoForProperties), contract.Properties);
                    RemoveProperty(nameof(LineInfo.LineNumber), contract.Properties);
                    RemoveProperty(nameof(LineInfo.LinePosition), contract.Properties);
                }

                return contract;
            }

            return _defaultContractResolver.ResolveContract(type);
        }

        private void RemoveProperty(string propertyName, JsonPropertyCollection properties)
        {
            JsonProperty jsonProperty = properties.GetProperty(propertyName, StringComparison.OrdinalIgnoreCase);

            if (jsonProperty != null)
            {
                properties.Remove(jsonProperty);
            }
        }
    }
}
