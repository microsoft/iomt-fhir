// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public static class LineAwareJsonObjectExtensions
    {
        public static LineInfo GetLineInfoForProperty(this LineAwareJsonObject lineAwareJsonObject, string property)
        {
            EnsureArg.IsNotNull(lineAwareJsonObject, nameof(lineAwareJsonObject));
            EnsureArg.IsNotNullOrWhiteSpace(property, nameof(property));

            if (lineAwareJsonObject.LineInfoForProperties != null &&
                lineAwareJsonObject.LineInfoForProperties.TryGetValue(property, out var lineInfo))
            {
                return lineInfo;
            }

            return null;
        }

        public static T SetLineInfoProperties<T>(this T lineAwareJsonObject, JObject sourceObject)
            where T : LineAwareJsonObject
        {
            EnsureArg.IsNotNull(lineAwareJsonObject, nameof(lineAwareJsonObject));
            EnsureArg.IsNotNull(sourceObject, nameof(sourceObject));

            foreach (var property in sourceObject.Properties())
            {
                if (property is IJsonLineInfo propertyLineInfo)
                {
                    if (propertyLineInfo != null)
                    {
                        lineAwareJsonObject.LineInfoForProperties[property.Name] = new LineInfo
                        {
                            LineNumber = propertyLineInfo.LineNumber,
                            LinePosition = propertyLineInfo.LinePosition,
                        };
                    }
                }
            }

            return lineAwareJsonObject;
        }
    }
}