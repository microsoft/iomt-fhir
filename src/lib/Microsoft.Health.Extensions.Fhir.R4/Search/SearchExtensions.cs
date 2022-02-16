// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Hl7.Fhir.Rest;

namespace Microsoft.Health.Extensions.Fhir.Search
{
    public static class SearchExtensions
    {
        public static Hl7.Fhir.Rest.SearchParams MatchOnId(this Hl7.Fhir.Rest.SearchParams searchParams, string id)
        {
            EnsureArg.IsNotNull(searchParams, nameof(searchParams));

            searchParams.Parameters.Add(new Tuple<string, string>(SearchParam.Id.ToString(), id));
            return searchParams;
        }

        public static Hl7.Fhir.Rest.SearchParams ToSearchPath<TResource>()
             where TResource : Hl7.Fhir.Model.Resource
        {
            var searchParams = new Hl7.Fhir.Rest.SearchParams();
            return searchParams;
        }

        public static Hl7.Fhir.Rest.SearchParams ForSubject<TResource>(this string subjectValue)
            where TResource : Hl7.Fhir.Model.Resource
        {
            return new Hl7.Fhir.Rest.SearchParams()
                .ForSubject<TResource>(subjectValue);
        }

        public static Hl7.Fhir.Rest.SearchParams ForSubject<TResource>(this Hl7.Fhir.Rest.SearchParams searchParams, string subjectValue)
            where TResource : Hl7.Fhir.Model.Resource
        {
            EnsureArg.IsNotNull(searchParams, nameof(searchParams));

            searchParams.Add(SearchParam.Subject.ToString(), $@"{typeof(TResource).Name}/{subjectValue}");
            return searchParams;
        }

        public static Hl7.Fhir.Rest.SearchParams SetCount(this Hl7.Fhir.Rest.SearchParams searchParams, int? count)
        {
            EnsureArg.IsNotNull(searchParams, nameof(searchParams));

            searchParams.Count = count;
            return searchParams;
        }

        /// <summary>
        /// Apply specified sort values to the search parameters.
        /// </summary>
        /// <param name="searchParams">The parameter collection to modify.</param>
        /// <param name="sortParameters">The parameters to sort by, applied in order. If null or empty no sorting will be applied.</param>
        /// <returns>The modified search parameter collection.</returns>
        /// <remarks>Fhir spec calls for indicating a descing sort using a minus, i.e. -date. Currently our fhir server implementation doesn't support sorting so values are ignored.</remarks>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#_sort"/>
        /// <seealso cref="https://github.com/Microsoft/fhir-server/blob/master/docs/Features.md#Search"/>
        public static Hl7.Fhir.Rest.SearchParams SortBy(this Hl7.Fhir.Rest.SearchParams searchParams, params string[] sortParameters)
        {
            foreach (var sortParam in sortParameters ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(sortParam))
                {
                    searchParams.OrderBy(sortParam);
                }
            }

            return searchParams;
        }

        /// <summary>
        /// Conditionally add search parameter for the specified value to the parameter collection.
        /// </summary>
        /// <param name="searchParams">The search parameter collection to modify.</param>
        /// <param name="searchParam">Search parameter to add.</param>
        /// <param name="paramValue">The value of to add to the collection. If value is null no search parameter will be added.</param>
        /// <param name="searchPrefix">Optional prefix for value matching.</param>
        /// <returns>The modified search parameter collection.</returns>
        public static Hl7.Fhir.Rest.SearchParams WhenParamValue(this Hl7.Fhir.Rest.SearchParams searchParams, SearchParam searchParam, object paramValue, SearchPrefix searchPrefix = null)
        {
            return searchParams.WhenParamValue(searchParam?.ToString(), paramValue, searchPrefix);
        }

        /// <summary>
        /// Conditionally add search parameter for the specified value to the parameter collection.
        /// </summary>
        /// <param name="searchParams">The search parameter collection to modify.</param>
        /// <param name="searchParam">Search parameter to add.</param>
        /// <param name="resourceRefValue">The resource reference add to the collection. If value is null no search parameter will be added.</param>
        /// <param name="searchPrefix">Optional prefix for value matching.</param>
        /// <returns>The modified search parameter collection.</returns>
        public static Hl7.Fhir.Rest.SearchParams WhenParamValue(this Hl7.Fhir.Rest.SearchParams searchParams, SearchParam searchParam, Hl7.Fhir.Model.ResourceReference resourceRefValue, SearchPrefix searchPrefix = null)
        {
            return searchParams.WhenParamValue(searchParam?.ToString(), resourceRefValue, searchPrefix);
        }

        /// <summary>
        /// Conditionally add search parameter for the specified value to the parameter collection.
        /// </summary>
        /// <param name="searchParams">The search parameter collection to modify.</param>
        /// <param name="searchParam">Search parameter to add.</param>
        /// <param name="paramValue">The value of the parameter to add to the collection. If value is null no search parameter will be added.</param>
        /// <param name="searchPrefix">Optional prefix for value matching.</param>
        /// <returns>The modified search parameter collection.</returns>
        public static Hl7.Fhir.Rest.SearchParams WhenParamValue(this Hl7.Fhir.Rest.SearchParams searchParams, string searchParam, object paramValue, SearchPrefix searchPrefix = null)
        {
            EnsureArg.IsNotNull(searchParams, nameof(searchParams));

            if (searchParam == null)
            {
                throw new ArgumentNullException(nameof(searchParam));
            }

            if (paramValue != null)
            {
                SearchPrefix prefix = searchPrefix ?? SearchPrefix.Empty;
                searchParams.Add(searchParam, $@"{searchPrefix}{paramValue}");
            }

            return searchParams;
        }

        /// <summary>
        /// Conditionally add search parameter for the specified value to the parameter collection.
        /// </summary>
        /// <param name="searchParams">The search parameter collection to modify.</param>
        /// <param name="searchParam">Search parameter to add.</param>
        /// <param name="resourceRefValue">The resource reference add to the collection. If value is null no search parameter will be added.</param>
        /// <param name="searchPrefix">Optional prefix for value matching.</param>
        /// <returns>The modified search parameter collection.</returns>
        public static Hl7.Fhir.Rest.SearchParams WhenParamValue(this Hl7.Fhir.Rest.SearchParams searchParams, string searchParam, Hl7.Fhir.Model.ResourceReference resourceRefValue, SearchPrefix searchPrefix = null)
        {
            return searchParams.WhenParamValue(searchParam, resourceRefValue?.Url.ToString(), searchPrefix);
        }

        public static Hl7.Fhir.Rest.SearchParams MatchOnAnyIdentifier(this Hl7.Fhir.Rest.SearchParams searchParams, IEnumerable<Hl7.Fhir.Model.Identifier> identifiers)
        {
            EnsureArg.IsNotNull(searchParams, nameof(searchParams));

            searchParams.Add(SearchParam.Identifier.ToString(), identifiers
                .Select(id => id.ToSearchToken())
                .CompositeOr());

            return searchParams;
        }

        public static Hl7.Fhir.Rest.SearchParams MatchOnAnyIdentifier(this IEnumerable<Hl7.Fhir.Model.Identifier> identifiers)
        {
            var searchParams = new Hl7.Fhir.Rest.SearchParams();
            searchParams.Add(SearchParam.Identifier.ToString(), identifiers
                .Select(id => id.ToSearchToken())
                .CompositeOr());

            return searchParams;
        }

        public static Hl7.Fhir.Rest.SearchParams MatchOnAllIdentifiers(this IEnumerable<Hl7.Fhir.Model.Identifier> identifiers)
        {
            var searchParams = new Hl7.Fhir.Rest.SearchParams();
            searchParams.Add(SearchParam.Identifier.ToString(), identifiers
                .Select(id => id.ToSearchToken())
                .CompositeAnd());

            return searchParams;
        }

        public static Hl7.Fhir.Rest.SearchParams ToSearchParams(this Hl7.Fhir.Model.Identifier identifier)
        {
            var searchParams = new Hl7.Fhir.Rest.SearchParams();
            searchParams.Add(SearchParam.Identifier.ToString(), identifier.ToSearchToken());
            return searchParams;
        }

        public static string ToSearchToken(this Hl7.Fhir.Model.Identifier identifier)
        {
            EnsureArg.IsNotNull(identifier, nameof(identifier));

            var token = string.Empty;
            if (!string.IsNullOrEmpty(identifier.System))
            {
                token += $"{identifier.System}|";
            }

            token += identifier.Value;
            return token;
        }

        public static string ToSearchQueryParameter(this Hl7.Fhir.Model.Identifier identifier)
        {
            EnsureArg.IsNotNull(identifier, nameof(identifier));

            return $"identifier={identifier.ToSearchToken()}";
        }
    }
}
