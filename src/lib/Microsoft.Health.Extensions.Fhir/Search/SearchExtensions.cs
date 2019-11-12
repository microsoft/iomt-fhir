// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Health.Extensions.Fhir.Search
{
    public static class SearchExtensions
    {
        /// <summary>
        /// Combine search tokens for an Or search according to the FHIR composite search specification.
        /// </summary>
        /// <param name="searchTokens">Enumerable of search tokens to combine.</param>
        /// <returns>A concatenated string of search terms joined for an Or search.</returns>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#combining"/>
        public static string CompositeOr(this IEnumerable<string> searchTokens)
        {
            return InternalBuildCompositeSearch(searchTokens, SearchCondition.CompositeOr);
        }

        /// <summary>
        /// Combine search tokens for an And search according to the FHIR composite search specification.
        /// </summary>
        /// <param name="searchTokens">Enumerable of search tokens to combine.</param>
        /// <returns>A concatenated string of search terms joined for an And search.</returns>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#combining"/>
        public static string CompositeAnd(this IEnumerable<string> searchTokens)
        {
            return InternalBuildCompositeSearch(searchTokens, SearchCondition.CompositeAnd);
        }

        private static string InternalBuildCompositeSearch(IEnumerable<string> searchTokens, SearchCondition searchCondition)
        {
            var safeTokens = searchTokens ?? Enumerable.Empty<string>();
            return string.Join(searchCondition.ToString(), searchTokens);
        }
    }
}
