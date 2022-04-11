// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class IotJsonPathLegacyMeasurementExtractor : LegacyMeasurementExtractor
    {
        private readonly IotJsonPathContentTemplate _template;

        public IotJsonPathLegacyMeasurementExtractor(
            IotJsonPathContentTemplate template)
            : base(new JsonPathCalculatedFunctionContentTemplateAdapter<IotJsonPathContentTemplate>(template), new JsonPathExpressionEvaluatorFactory())
        {
            _template = EnsureArg.IsNotNull(template, nameof(template));
        }

        protected override DateTime? GetTimestamp(JToken token) => EvalExpression<DateTime?>(token, nameof(Template.TimestampExpression), true, Template.TimestampExpression, new TemplateExpression(_template.AlternateTimestampExpression, TemplateExpressionLanguage.JsonPath));
    }
}
