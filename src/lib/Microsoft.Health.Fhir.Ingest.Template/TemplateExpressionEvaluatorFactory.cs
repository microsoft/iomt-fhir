// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using DevLab.JmesPath;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateExpressionEvaluatorFactory : IExpressionEvaluatorFactory
    {
        private readonly JmesPath _jmesPath;

        public TemplateExpressionEvaluatorFactory()
            : this(new JmesPath())
        {
        }

        public TemplateExpressionEvaluatorFactory(JmesPath jmesPath)
        {
            _jmesPath = EnsureArg.IsNotNull(jmesPath, nameof(jmesPath));
        }

        public IExpressionEvaluator Create(TemplateExpression expression)
        {
            EnsureArg.IsNotNullOrWhiteSpace(expression?.Value, nameof(expression.Value));
            EnsureArg.IsNotNull(expression?.Language, nameof(expression.Language));

            var lineInfo = expression.GetLineInfoForProperty("value") ?? expression;
            return expression.Language switch
            {
                TemplateExpressionLanguage.JsonPath => new JsonPathExpressionEvaluator(expression.Value, lineInfo),
                TemplateExpressionLanguage.JmesPath => new JmesPathExpressionEvaluator(_jmesPath, expression.Value, lineInfo),
                _ => throw new TemplateExpressionException($"Unsupported Expression Language: {expression?.Language}", lineInfo)
            };
        }
    }
}