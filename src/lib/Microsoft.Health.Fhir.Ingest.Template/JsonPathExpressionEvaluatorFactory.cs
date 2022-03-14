// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    /// <summary>
    /// A simple expression evaluator factory which always evaluates expressions using JsonPath
    /// </summary>
    public class JsonPathExpressionEvaluatorFactory : IExpressionEvaluatorFactory
    {
        public IExpressionEvaluator Create(TemplateExpression expression)
        {
            EnsureArg.IsNotNullOrWhiteSpace(expression?.Value, nameof(expression.Value));

            var expressionLanguage = expression.Language ?? TemplateExpressionLanguage.JsonPath;
            if (expressionLanguage != TemplateExpressionLanguage.JsonPath)
            {
                throw new TemplateExpressionException($"Unsupported Expression Language {expressionLanguage}. Only JsonPath is supported.");
            }

            return new JsonPathExpressionEvaluator(expression.Value);
        }
    }
}
