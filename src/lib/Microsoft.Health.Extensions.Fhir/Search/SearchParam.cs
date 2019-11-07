// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Extensions.Fhir.Search
{
    /// <summary>
    /// Defines common FHIR supported search parameters across resources.
    /// </summary>
    public class SearchParam
    {
        private SearchParam(string searchParamValue)
        {
            SearchParamValue = searchParamValue;
        }

        /// <summary>
        /// Generic date search parameter.
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#date" />
        public static SearchParam Date { get; } = new SearchParam("date");

        /// <summary>
        /// Subject reference search parameter.
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#reference" />
        public static SearchParam Subject { get; } = new SearchParam("subject");

        public static SearchParam Code { get; } = new SearchParam("code");

        public static SearchParam Coding { get; } = new SearchParam("coding");

        public static SearchParam Duration { get; } = new SearchParam("duration");

        public static SearchParam System { get; } = new SearchParam("system");

        public static SearchParam Identifier { get; } = new SearchParam("identifier");

        /// <summary>
        /// Generic id search parameter
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#id"/>
        public static SearchParam Id { get; } = new SearchParam("_id");

        /// <summary>
        /// Generic lastUpdated search parameter
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#lastUpdated"/>
        public static SearchParam LastUpdated { get; } = new SearchParam("_lastUpdated");

        /// <summary>
        /// Generic tag search parameter
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#tag"/>
        public static SearchParam Tag { get; } = new SearchParam("_tag");

        /// <summary>
        /// Generic profile search parameter
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#profile"/>
        public static SearchParam Profile { get; } = new SearchParam("_profile");

        /// <summary>
        /// Generic text search parameter
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#text"/>
        public static SearchParam Text { get; } = new SearchParam("_text");

        /// <summary>
        /// Generic content search parameter
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#content"/>
        public static SearchParam Content { get; } = new SearchParam("_content");

        /// <summary>
        /// Generic list search parameter
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#list"/>
        public static SearchParam List { get; } = new SearchParam("_list");

        /// <summary>
        /// Generic has search parameter
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#has"/>
        public static SearchParam Has { get; } = new SearchParam("_has");

        /// <summary>
        /// Generic type search parameter
        /// </summary>
        /// <seealso cref="https://www.hl7.org/fhir/search.html#_type"/>
        public static SearchParam Type { get; } = new SearchParam("_type");

        private string SearchParamValue { get; set; }

        public override string ToString()
        {
            return SearchParamValue;
        }
    }
}
