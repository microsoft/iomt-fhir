// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Extensions.Fhir.Search
{
    /// <summary>
    /// Defines common search prefixes that can be applied before value to indicate how values should be matched.
    /// </summary>
    /// <seealso cref="https://www.hl7.org/fhir/search.html#prefix"/>
    public class SearchPrefix
    {
        private SearchPrefix(string searchPrefixValue)
        {
            Value = searchPrefixValue;
        }

        /// <summary>
        /// The range of the search value fully contains the range of the target value.
        /// </summary>
        public static SearchPrefix Equal { get; } = new SearchPrefix("eq");

        /// <summary>
        /// The range of the search value does not fully contain the range of the target value.
        /// </summary>
        public static SearchPrefix NotEqual { get; } = new SearchPrefix("ne");

        /// <summary>
        /// The range above the search value intersects (i.e. overlaps) with the range of the target value.
        /// </summary>
        public static SearchPrefix GreaterThan { get; } = new SearchPrefix("gt");

        /// <summary>
        /// The range below the search value intersects (i.e. overlaps) with the range of the target value.
        /// </summary>
        public static SearchPrefix LessThan { get; } = new SearchPrefix("lt");

        /// <summary>
        /// The range above the search value intersects (i.e. overlaps) with the range of the target value, or the range of the search value fully contains the range of the target value.
        /// </summary>
        public static SearchPrefix GreaterThanOrEqual { get; } = new SearchPrefix("ge");

        /// <summary>
        /// The range below the search value intersects (i.e. overlaps) with the range of the target value or the range of the search value fully contains the range of the target value.
        /// </summary>
        public static SearchPrefix LessThanOrEqual { get; } = new SearchPrefix("le");

        /// <summary>
        /// The range of the search value does not overlap with the range of the target value, and the range above the search value contains the range of the target value.
        /// </summary>
        public static SearchPrefix StartsAfter { get; } = new SearchPrefix("sa");

        /// <summary>
        /// The range of the search value does overlap not with the range of the target value, and the range below the search value contains the range of the target value.
        /// </summary>
        public static SearchPrefix EndsBefore { get; } = new SearchPrefix("eb");

        public static SearchPrefix SortDescending { get; } = new SearchPrefix("-");

        public static SearchPrefix Empty { get; } = new SearchPrefix(string.Empty);

        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
