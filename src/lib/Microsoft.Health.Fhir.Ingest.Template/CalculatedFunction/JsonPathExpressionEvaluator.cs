// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public class JsonPathExpressionEvaluator : IExpressionEvaluator
    {
        private readonly string _jsonPathExpression;

        public JsonPathExpressionEvaluator(string jsonPathExpression)
        {
            _jsonPathExpression = EnsureArg.IsNotNullOrWhiteSpace(jsonPathExpression, nameof(jsonPathExpression));
        }

        public JToken SelectToken(JToken data)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            return data.SelectToken(_jsonPathExpression);
        }

        public IEnumerable<JToken> SelectTokens(JToken data)
        {
            return data.SelectTokens(_jsonPathExpression);
        }
    }
}
