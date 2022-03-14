// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    /// <summary>
    /// An instance of a MeasurementExtractor which does not wrap its match tokens inside of the original event data. This is the
    /// legacy behavior.
    /// </summary>
    public class LegacyMeasurementExtractor : MeasurementExtractor
    {
        public LegacyMeasurementExtractor(
            CalculatedFunctionContentTemplate template,
            IExpressionEvaluatorFactory expressionEvaluatorFactory)
            : base(template, expressionEvaluatorFactory)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<JToken> MatchTypeTokens(JObject token)
        {
            EnsureArg.IsNotNull(token, nameof(token));
            var evaluator = CreateRequiredExpressionEvaluator(Template.TypeMatchExpression, nameof(Template.TypeMatchExpression));

            return evaluator.SelectTokens(token);
        }
    }
}
