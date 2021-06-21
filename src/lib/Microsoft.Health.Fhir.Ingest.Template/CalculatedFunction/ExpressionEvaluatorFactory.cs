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
        private JmesPath _jmesPath;

        public ExpressionEvaluatorFactory()
        {
            _jmesPath = new JmesPath();

            /*
             * register additional custom functions.
             *
             * TODO: Do we want to allow customers to register additional, custom functions in the OSS project? Possibly via dependency injection?
             *
             */
        }

        public IExpressionEvaluator Create(Expression expression)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(expression?.Value, nameof(expression.Value));

            switch (expression.Language)
            {
                case ExpressionLanguage.JsonPath:
                    return new JsonPathExpressionEvaluator(expression.Value);
                case ExpressionLanguage.JMESPath:
                    try
                    {
                        var jmesPathExpression = _jmesPath.Parse(expression.Value);
                        return new JMESPathExpressionEvaluator(jmesPathExpression, expression);
                    }
                    catch (Exception e)
                    {
                        throw new ExpressionException("Unable to parse JMESPath expression", e);
                    }

                default:
                    throw new ArgumentException($"Unsupported Expression Language {expression.Language}");
            }
        }
    }
}