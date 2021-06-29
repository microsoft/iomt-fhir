// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class ModelExtensions
    {
        private static ConcurrentDictionary<string, Regex> _idMatcherRegexCache = new ConcurrentDictionary<string, Regex>();

        public static Hl7.Fhir.Model.ResourceReference ToReference<TResource>(this TResource resource)
            where TResource : Hl7.Fhir.Model.Resource
        {
            if (resource == null)
            {
                return null;
            }

            return new Hl7.Fhir.Model.ResourceReference($@"{typeof(TResource).Name}/{resource.Id}");
        }

        public static Hl7.Fhir.Model.ResourceReference ToReference<TResource>(this string id)
            where TResource : Hl7.Fhir.Model.Resource
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return new Hl7.Fhir.Model.ResourceReference($@"{typeof(TResource).Name}/{id}");
        }

        /// <summary>
        /// Returns the id for the specified resource type from the provided reference.  If the reference doesn't match the provided type null will be returned.
        /// </summary>
        /// <typeparam name="TResource">Expected resource type the reference represents.</typeparam>
        /// <param name="reference">Resource  reference to extract the id from.</param>
        /// <returns>The id for the specified resource type if it exists, null otherwise.</returns>
        public static string GetId<TResource>(this Hl7.Fhir.Model.ResourceReference reference)
        {
            string id;
            var referenceType = typeof(TResource).Name;

            if (reference?.Reference == null)
            {
                return null;
            }

            var idMatcherRegex = _idMatcherRegexCache.GetOrAdd(referenceType, new Regex(referenceType + @"\/([A-Za-z0-9_.\-~#]{1,64})", RegexOptions.Compiled));

            // Reference should be in the form: ResourceType/Identifier
            // If there is a match, the 2nd group will contain the identifier.
            var matches = idMatcherRegex.Match(reference.Reference);
            if (matches?.Groups?.Count != 2)
            {
                return null;
            }
            else
            {
                id = matches?.Groups?[1].Value;
                return id;
            }
        }
    }
}
