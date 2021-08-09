// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using EnsureThat;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CalculatedFunctionContentTemplateFactory : HandlerProxyTemplateFactory<TemplateContainer, IContentTemplate>
    {
        private const string TargetTypeName = "CalculatedContent";
        private readonly ITelemetryLogger _logger;
        private readonly IExpressionEvaluatorFactory _expressionEvaluatorFactory;

        public CalculatedFunctionContentTemplateFactory(
            IExpressionEvaluatorFactory expressionEvaluatorFactory,
            ITelemetryLogger logger)
        {
            _expressionEvaluatorFactory = EnsureArg.IsNotNull(expressionEvaluatorFactory, nameof(expressionEvaluatorFactory));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
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
            calculatedFunctionTemplate.EnsureExpressionLanguageIsSet();
            var measurementExtractor = new MeasurementExtractor(calculatedFunctionTemplate, CreateExpressionEvaluatorFactory(calculatedFunctionTemplate));
            return measurementExtractor;
        }

        private IExpressionEvaluatorFactory CreateExpressionEvaluatorFactory(CalculatedFunctionContentTemplate template)
        {
            var evaluatorCache = new Dictionary<string, IExpressionEvaluator>();

            AddExpression(evaluatorCache, template.TypeMatchExpression, nameof(template.TypeMatchExpression), true);
            AddExpression(evaluatorCache, template.DeviceIdExpression, nameof(template.DeviceIdExpression), true);
            AddExpression(evaluatorCache, template.PatientIdExpression, nameof(template.PatientIdExpression));
            AddExpression(evaluatorCache, template.EncounterIdExpression, nameof(template.EncounterIdExpression));
            AddExpression(evaluatorCache, template.TimestampExpression, nameof(template.TimestampExpression));
            AddExpression(evaluatorCache, template.CorrelationIdExpression, nameof(template.CorrelationIdExpression));

            foreach (var value in template.Values)
            {
                AddExpression(evaluatorCache, value.ValueExpression, value.ValueName, value.Required);
            }

            return new CachingExpressionEvaluatorFactory(new ReadOnlyDictionary<string, IExpressionEvaluator>(evaluatorCache));
        }

        private void AddExpression(
            IDictionary<string, IExpressionEvaluator> cache,
            TemplateExpression expression,
            string expressionName,
            bool isRequired = false)
        {
            if (expression != null)
            {
                cache[expression.GetId()] = _expressionEvaluatorFactory.Create(expression);
                _logger.LogTrace($"Using {expression.Value} for expression [{expressionName}]");
            }
            else if (isRequired)
            {
                throw new TemplateExpressionException($"Unable to create the template; the expression for [{expressionName}] is missing");
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
