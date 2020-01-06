// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class BundleExtensions
    {
        public static IEnumerable<TResource> ReadFromBundle<TResource>(this Resource resource)
            where TResource : Resource
        {
            return ReadFromBundle<TResource>(resource as Bundle);
        }

        public static IEnumerable<TResource> ReadFromBundle<TResource>(this Bundle bundle, int? count = null)
            where TResource : Resource
        {
            if ((bundle?.Entry?.Count ?? 0) == 0)
            {
                yield break;
            }

            var iterator = bundle.Entry.Select(e => e.Resource).OfType<TResource>();
            if (count != null)
            {
                iterator = iterator.Take(count.Value);
            }

            foreach (var item in iterator)
            {
                yield return item;
            }
        }

        public static async Task<TResource> ReadOneFromBundleWithContinuationAsync<TResource>(this Bundle bundle, IFhirClient fhirClient, bool throwOnMultipleFound = true)
            where TResource : Resource, new()
        {
            if (bundle == null)
            {
                return null;
            }

            var resources = await bundle?.ReadFromBundleWithContinuationAsync<TResource>(fhirClient, 2);

            var resourceCount = resources.Count();
            if (resourceCount == 0)
            {
                return null;
            }

            if (throwOnMultipleFound && resourceCount > 1)
            {
                throw new MultipleResourceFoundException<TResource>();
            }

            return resources.FirstOrDefault();
        }

        /// <summary>
        /// Returns the count of enteries in the first bundled (no continuation). If the bundle or entry collection is null zero is returned.
        /// </summary>
        /// <param name="bundle">The bundle to return the entry count of.</param>
        /// <returns>The number of items returned in the initial bundle entry collection.</returns>
        public static int EntryCount(this Bundle bundle)
        {
            return bundle?.Entry?.Count ?? 0;
        }

        public static async Task<IEnumerable<TResource>> ReadFromBundleWithContinuationAsync<TResource>(this Bundle bundle, IFhirClient fhirClient, int? count = null)
            where TResource : Resource
        {
            EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));

            var resources = new List<TResource>();
            while (bundle != null)
            {
                foreach (var r in bundle.ReadFromBundle<TResource>(count))
                {
                    if (count == 0)
                    {
                        break;
                    }

                    resources.Add(r);
                    if (count != null)
                    {
                        count--;
                    }
                }

                bundle = await fhirClient.ContinueAsync(bundle).ConfigureAwait(false);
            }

            return resources;
        }

        public static async Task<IEnumerable<TResource>> SearchWithContinuationAsync<TResource>(this IFhirClient fhirClient, SearchParams searchParams)
            where TResource : Resource
        {
            EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));
            EnsureArg.IsNotNull(searchParams, nameof(searchParams));

            var result = await fhirClient.SearchAsync<TResource>(searchParams).ConfigureAwait(false) as Bundle;
            return await result.ReadFromBundleWithContinuationAsync<TResource>(fhirClient, searchParams.Count).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<TResource>> GetWithContinuationAsync<TResource>(this IFhirClient fhirClient, Uri resourceUri, int? count = null)
            where TResource : Resource
        {
            EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));

            var result = await fhirClient.GetAsync(resourceUri).ConfigureAwait(false) as Bundle;
            return await result.ReadFromBundleWithContinuationAsync<TResource>(fhirClient, count).ConfigureAwait(false);
        }
    }
}
