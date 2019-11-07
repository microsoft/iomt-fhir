// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Extensions.Fhir.Search
{
    public class SearchCondition
    {
        private SearchCondition(string searchParameterValue)
        {
            SearchParameterValue = searchParameterValue;
        }

        public static SearchCondition CompositeOr { get; } = new SearchCondition(",");

        public static SearchCondition CompositeAnd { get; } = new SearchCondition("&");

        private string SearchParameterValue { get; set; }

        public override string ToString()
        {
            return SearchParameterValue;
        }
    }
}
