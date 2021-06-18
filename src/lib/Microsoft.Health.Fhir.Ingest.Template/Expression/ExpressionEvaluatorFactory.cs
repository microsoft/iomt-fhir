// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using DevLab.JmesPath;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template.Expression
{
    public class ExpressionEvaluatorFactory : IExpressionEvaluatorFactory
    {
        private JmesPath _jmesPath;

        public ExpressionEvaluatorFactory()
        {
            _jmesPath = new JmesPath();

            // register additional functions.... Need to figure out how to allow external customers to add in their own functions
        }

        public IExpressionEvaluator Create(Expression expression)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(expression?.Value, nameof(expression.Value));

            switch (expression.Language)
            {
                case ExpressionLanguage.JsonPath:
                    return null;
                case ExpressionLanguage.JMESPath:
                    var jmesPathExpression = _jmesPath.Parse(expression.Value);
                    return new JMESPathExpressionEvaluator(jmesPathExpression, expression);
                default:
                    throw new ArgumentException($"Unsupported Expression Language {expression.Language}");
            }
        }
    }
}