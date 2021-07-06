// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public static class ExpressionExtensions
    {
        private const string _defaultLanguage = "default";

        public static string GetId(this Expression expression)
        {
            var language = expression.Language.ToString() ?? _defaultLanguage;
            return $"{expression.Value}-{language}";
        }
    }
}
