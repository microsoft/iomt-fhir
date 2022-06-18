// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using DevLab.JmesPath;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class JmesPathExpressionEvaluator : IExpressionEvaluator
    {
        private readonly JmesPath.Expression _jmespathExpression;
        private readonly ILineInfo _lineInfo;

        public JmesPathExpressionEvaluator(JmesPath jmesPath, string expression, ILineInfo lineInfo)
        {
            EnsureArg.IsNotNull(jmesPath, nameof(jmesPath));
            EnsureArg.IsNotNullOrWhiteSpace(expression, nameof(expression));
            _lineInfo = EnsureArg.IsNotNull(lineInfo, nameof(lineInfo));

            try
            {
                _jmespathExpression = jmesPath.Parse(expression);
            }
            catch (Exception e)
            {
                throw new TemplateExpressionException($"The following JmesPath expression could not be parsed: {expression}. Cause: {e.Message}", e, _lineInfo);
            }
        }

        public JToken SelectToken(JToken data)
        {
            EnsureArg.IsNotNull(data);
            var jmesPathArgument = _jmespathExpression.Transform(data);

            if (jmesPathArgument.IsProjection && jmesPathArgument.Projection.Length > 1)
            {
                throw new TemplateExpressionException($"Multiple tokens were returned using expression ${_jmespathExpression}", _lineInfo);
            }

            var resultAsToken = jmesPathArgument.AsJToken();
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
