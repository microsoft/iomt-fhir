// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public class CachingExpressionEvaluatorFactory : IExpressionEvaluatorFactory
    {
        private IExpressionEvaluatorFactory _wrappedExpressionEvaluatorFactory;
        private IDictionary<string, IExpressionEvaluator> _expressionCache;

        public CachingExpressionEvaluatorFactory()
            : this(new ExpressionEvaluatorFactory())
        {
        }

        public CachingExpressionEvaluatorFactory(IExpressionEvaluatorFactory expressionEvaluatorFactory)
        {
            _wrappedExpressionEvaluatorFactory = EnsureArg.IsNotNull(expressionEvaluatorFactory, nameof(expressionEvaluatorFactory));
            _expressionCache = new Dictionary<string, IExpressionEvaluator>();
        }

        public IExpressionEvaluator Create(Expression expression)
        {
            if (_expressionCache.TryGetValue(expression.Id, out IExpressionEvaluator cachedExpressionEvaluator))
            {
                return cachedExpressionEvaluator;
            }
            else
            {
                var newExpressionEvaluator = _wrappedExpressionEvaluatorFactory.Create(expression);
                _expressionCache[expression.Id] = newExpressionEvaluator;
                return newExpressionEvaluator;
            }
        }
    }
}
