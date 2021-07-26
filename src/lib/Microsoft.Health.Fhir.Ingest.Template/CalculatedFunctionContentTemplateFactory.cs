// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevLab.JmesPath;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CalculatedFunctionContentTemplateFactory : HandlerProxyTemplateFactory<TemplateContainer, IContentTemplate>
    {
        private const string TargetTypeName = "CalculatedContent";
        private JmesPath _jmesPath;

        public CalculatedFunctionContentTemplateFactory()
        {
            /*
             * TODO Load and register additional custom JmesPath functions. For now, simply create the basic JmesPath object
             */
            _jmesPath = new JmesPath();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message")]
        public override IContentTemplate Create(TemplateContainer jsonTemplate)
        {
            EnsureArg.IsNotNull(jsonTemplate, nameof(jsonTemplate));

            if (!jsonTemplate.MatchTemplateName(TargetTypeName))
            {
                return null;
            }

            if (jsonTemplate.Template?.Type != JTokenType.Object)
            {
                throw new InvalidTemplateException($"Expected an object for the template property value for template type {TargetTypeName}.");
            }

            var calculatedFunctionTemplate = jsonTemplate.Template.ToValidTemplate<CalculatedFunctionContentTemplate>();
            var measurementExtractor = new MeasurementExtractor(calculatedFunctionTemplate, CreateExpressionEvaluatorFactory(calculatedFunctionTemplate));
            return measurementExtractor;
        }

        private IExpressionEvaluatorFactory CreateExpressionEvaluatorFactory(CalculatedFunctionContentTemplate template)
        {
            var evaluatorCache = new Dictionary<string, IExpressionEvaluator>();
            IExpressionEvaluatorFactory expressionEvaluator = new TemplateExpressionEvaluatorFactory(_jmesPath, template.DefaultExpressionLanguage);

            AddExpression(evaluatorCache, template.TypeMatchExpression, expressionEvaluator);
            AddExpression(evaluatorCache, template.DeviceIdExpression, expressionEvaluator);
            AddExpression(evaluatorCache, template.PatientIdExpression, expressionEvaluator);
            AddExpression(evaluatorCache, template.EncounterIdExpression, expressionEvaluator);
            AddExpression(evaluatorCache, template.TimestampExpression, expressionEvaluator);
            AddExpression(evaluatorCache, template.CorrelationIdExpression, expressionEvaluator);

            foreach (var valueExpression in template.Values)
            {
                AddExpression(evaluatorCache, valueExpression, expressionEvaluator);
            }

            return new CachingExpressionEvaluatorFactory(new ReadOnlyDictionary<string, IExpressionEvaluator>(evaluatorCache));
        }

        private void AddExpression(
            IDictionary<string, IExpressionEvaluator> cache,
            TemplateExpression expression,
            IExpressionEvaluatorFactory expressionEvaluator)
        {
            if (expression != null)
            {
                cache[expression.GetId()] = expressionEvaluator.Create(expression);
            }
        }

        private class CachingExpressionEvaluatorFactory : IExpressionEvaluatorFactory
        {
            private IReadOnlyDictionary<string, IExpressionEvaluator> _expressionCache;

            public CachingExpressionEvaluatorFactory(IReadOnlyDictionary<string, IExpressionEvaluator> expressionCache)
            {
                _expressionCache = EnsureArg.IsNotNull(expressionCache, nameof(expressionCache));
            }

            public IExpressionEvaluator Create(TemplateExpression expression)
            {
                EnsureArg.IsNotNull(expression, nameof(expression));

                return _expressionCache[expression.GetId()];
            }
        }
    }
}
