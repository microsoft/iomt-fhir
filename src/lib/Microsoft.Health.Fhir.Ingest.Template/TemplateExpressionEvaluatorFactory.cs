// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using DevLab.JmesPath;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateExpressionEvaluatorFactory : IExpressionEvaluatorFactory
    {
        private readonly JmesPath _jmesPath;
        private readonly TemplateExpressionLanguage _defaultExpressionLanguage;

        public TemplateExpressionEvaluatorFactory()
            : this(new JmesPath(), TemplateExpressionLanguage.JsonPath)
        {
        }

        public TemplateExpressionEvaluatorFactory(TemplateExpressionLanguage defaultLanguage)
            : this(new JmesPath(), defaultLanguage)
        {
        }

        public TemplateExpressionEvaluatorFactory(JmesPath jmesPath, TemplateExpressionLanguage defaultLanguage)
        {
            _jmesPath = EnsureArg.IsNotNull(jmesPath, nameof(jmesPath));
            _defaultExpressionLanguage = defaultLanguage;
        }

        public IExpressionEvaluator Create(TemplateExpression expression)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(expression?.Value, nameof(expression.Value));

            var expressionLanguage = expression.Language ?? _defaultExpressionLanguage;

            return expressionLanguage switch
            {
                TemplateExpressionLanguage.JsonPath => new JsonPathExpressionEvaluator(expression.Value),
                TemplateExpressionLanguage.JmesPath => new JmesPathExpressionEvaluator(_jmesPath, expression.Value),
                _ => throw new ArgumentException($"Unsupported Expression Language {expressionLanguage}")
            };
        }
    }
}