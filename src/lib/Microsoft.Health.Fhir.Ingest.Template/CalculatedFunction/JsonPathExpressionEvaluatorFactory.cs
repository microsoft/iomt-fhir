// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    /// <summary>
    /// A simple expression evaluator factory which always evaluates expressions using JsonPath
    /// </summary>
    public class JsonPathExpressionEvaluatorFactory : IExpressionEvaluatorFactory
    {
        public IExpressionEvaluator Create(Expression expression)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(expression?.Value, nameof(expression.Value));

            var expressionLanguage = expression.Language ?? ExpressionLanguage.JsonPath;
            if (expressionLanguage != ExpressionLanguage.JsonPath)
            {
                throw new ArgumentException($"Unsupported Expression Language {expressionLanguage}. Only JsonPath is supported.");
            }

            return new JsonPathExpressionEvaluator(expression.Value);
        }
    }
}
