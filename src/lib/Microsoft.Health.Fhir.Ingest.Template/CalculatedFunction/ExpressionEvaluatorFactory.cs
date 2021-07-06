// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using DevLab.JmesPath;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public class ExpressionEvaluatorFactory : IExpressionEvaluatorFactory
    {
        private readonly JmesPath _jmesPath;
        private readonly ExpressionLanguage _defaultExpressionLanguage;

        public ExpressionEvaluatorFactory()
            : this(new JmesPath(), ExpressionLanguage.JsonPath)
        {
        }

        public ExpressionEvaluatorFactory(ExpressionLanguage defaultLanguage)
            : this(new JmesPath(), defaultLanguage)
        {
        }

        public ExpressionEvaluatorFactory(JmesPath jmesPath, ExpressionLanguage defaultLanguage)
        {
            _jmesPath = EnsureArg.IsNotNull(jmesPath, nameof(jmesPath));
            _defaultExpressionLanguage = defaultLanguage;
        }

        public IExpressionEvaluator Create(Expression expression)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(expression?.Value, nameof(expression.Value));

            var expressionLanguage = expression.Language ?? _defaultExpressionLanguage;

            switch (expressionLanguage)
            {
                case ExpressionLanguage.JsonPath:
                    return new JsonPathExpressionEvaluator(expression.Value);
                case ExpressionLanguage.JmesPath:
                    try
                    {
                        var jmesPathExpression = _jmesPath.Parse(expression.Value);
                        return new JMESPathExpressionEvaluator(jmesPathExpression);
                    }
                    catch (Exception e)
                    {
                        throw new ExpressionException("Unable to parse JMESPath expression", e);
                    }

                default:
                    throw new ArgumentException($"Unsupported Expression Language {expressionLanguage}");
            }
        }
    }
}