// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public class CalculatedFunctionContentTemplateFactory : HandlerProxyTemplateFactory<TemplateContainer, IContentTemplate>
    {
        private const string TargetTypeName = "CalculatedFunctionContent";
        private IExpressionEvaluatorFactory _expressionEvaluatorFactory;

        public CalculatedFunctionContentTemplateFactory()
            : this(new ExpressionEvaluatorFactory())
        {
        }

        public CalculatedFunctionContentTemplateFactory(IExpressionEvaluatorFactory expressionEvaluatorFactory)
        {
            _expressionEvaluatorFactory = EnsureArg.IsNotNull(expressionEvaluatorFactory, nameof(expressionEvaluatorFactory));
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
            calculatedFunctionTemplate.ExpressionEvaluatorFactory = CreateExpressionEvaluatorFactory(calculatedFunctionTemplate);
            return calculatedFunctionTemplate;
        }

        private IExpressionEvaluatorFactory CreateExpressionEvaluatorFactory(CalculatedFunctionContentTemplate template)
        {
            var evaluatorCache = new Dictionary<string, IExpressionEvaluator>();
            AddExpression(evaluatorCache, template.TypeMatchExpression, template.DefaultExpressionLanguage);
            AddExpression(evaluatorCache, template.DeviceIdExpression, template.DefaultExpressionLanguage);
            AddExpression(evaluatorCache, template.PatientIdExpression, template.DefaultExpressionLanguage);
            AddExpression(evaluatorCache, template.EncounterIdExpression, template.DefaultExpressionLanguage);
            AddExpression(evaluatorCache, template.TimestampExpression, template.DefaultExpressionLanguage);
            AddExpression(evaluatorCache, template.CorrelationIdExpression, template.DefaultExpressionLanguage);

            foreach (var valueExpression in template.Values)
            {
                AddExpression(evaluatorCache, valueExpression, template.DefaultExpressionLanguage);
            }

            return new CachingExpressionEvaluatorFactory(new ReadOnlyDictionary<string, IExpressionEvaluator>(evaluatorCache));
        }

        private void AddExpression(IDictionary<string, IExpressionEvaluator> cache, Expression expression, ExpressionLanguage defaultLanguage)
        {
            if (expression != null)
            {
                cache[expression.GetId()] = _expressionEvaluatorFactory.Create(expression, expression.Language ?? defaultLanguage);
            }
        }
    }
}
