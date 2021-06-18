// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using DevLab.JmesPath;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Expression
{
    public class JMESPathExpressionEvaluator : IExpressionEvaluator
    {
        private JmesPath.Expression _jmespathExpression;

        public JMESPathExpressionEvaluator(
            JmesPath.Expression jmespathExpression,
            Expression templateExpression)
        {
            _jmespathExpression = EnsureArg.IsNotNull(jmespathExpression, nameof(jmespathExpression));
        }

        public JToken Evaluate(JToken data)
        {
            return _jmespathExpression.Transform(data).AsJToken();
        }
    }
}
