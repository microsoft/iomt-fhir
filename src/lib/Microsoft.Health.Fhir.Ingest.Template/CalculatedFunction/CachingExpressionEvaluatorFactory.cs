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
        private IReadOnlyDictionary<string, IExpressionEvaluator> _expressionCache;

        public CachingExpressionEvaluatorFactory(IReadOnlyDictionary<string, IExpressionEvaluator> expressionCache)
        {
            _expressionCache = EnsureArg.IsNotNull(expressionCache, nameof(expressionCache));
        }

        public IExpressionEvaluator Create(Expression expression, ExpressionLanguage defaultLanguage = ExpressionLanguage.JsonPath)
        {
            EnsureArg.IsNotNull(expression, nameof(expression));

            return _expressionCache[expression.GetId()];
        }
    }
}
