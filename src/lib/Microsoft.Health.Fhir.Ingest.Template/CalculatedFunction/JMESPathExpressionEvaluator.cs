// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using DevLab.JmesPath;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public class JMESPathExpressionEvaluator : IExpressionEvaluator
    {
        private JmesPath.Expression _jmespathExpression;
        private Expression _expression;

        public JMESPathExpressionEvaluator(
            JmesPath.Expression jmespathExpression,
            Expression templateExpression)
        {
            _jmespathExpression = EnsureArg.IsNotNull(jmespathExpression, nameof(jmespathExpression));
            _expression = EnsureArg.IsNotNull(templateExpression, nameof(templateExpression));
        }

        public JToken SelectToken(JToken data)
        {
            var result = _jmespathExpression.Transform(data).AsJToken();

            if (result.Type == JTokenType.Array)
            {
                throw new ExpressionException($"Multiple tokens were returned using expression ${_expression.Value}");
            }

            return result;
        }

        public IEnumerable<JToken> SelectTokens(JToken data)
        {
            var result = _jmespathExpression.Transform(data).AsJToken();

            if (result.Type != JTokenType.Array)
            {
                throw new ExpressionException($"Expected result to be a collection when using expression: {_expression.Value}");
            }

            return result;
        }
    }
}
