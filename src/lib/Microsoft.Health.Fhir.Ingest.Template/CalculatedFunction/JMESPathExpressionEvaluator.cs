// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            EnsureArg.IsNotNull(data);
            var jmePathArgument = _jmespathExpression.Transform(data);

            if (jmePathArgument.IsProjection && jmePathArgument.Projection.Length > 1)
            {
                throw new ExpressionException($"Multiple tokens were returned using expression ${_expression.Value}");
            }

            var resultAsToken = jmePathArgument.AsJToken();
            if (resultAsToken.Type == JTokenType.Null)
            {
                return null;
            }

            return resultAsToken;
        }

        public IEnumerable<JToken> SelectTokens(JToken data)
        {
            EnsureArg.IsNotNull(data);
            var jmePathArgument = _jmespathExpression.Transform(data);

            if (jmePathArgument.IsProjection)
            {
                foreach (var arg in jmePathArgument.Projection)
                {
                    yield return arg.AsJToken();
                }
            }
            else
            {
                var resultAsToken = jmePathArgument.AsJToken();
                if (resultAsToken.Type == JTokenType.Null)
                {
                    yield break;
                }

                yield return resultAsToken;
            }
        }
    }
}
