// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class MeasurementExtractor : IContentTemplate
    {
        public const string MatchedToken = "matchedToken";

        public MeasurementExtractor(
            CalculatedFunctionContentTemplate template,
            IExpressionEvaluatorFactory expressionEvaluatorFactory)
        {
            Template = EnsureArg.IsNotNull(template, nameof(template));
            ExpressionEvaluatorFactory = EnsureArg.IsNotNull(expressionEvaluatorFactory, nameof(expressionEvaluatorFactory));
        }

        public CalculatedFunctionContentTemplate Template { get; private set; }

        public IExpressionEvaluatorFactory ExpressionEvaluatorFactory { get; private set; }

        public virtual IEnumerable<Measurement> GetMeasurements(JToken token)
        {
            EnsureArg.IsNotNull(token, nameof(token));
            EnsureArg.IsOfType(token, typeof(JObject), nameof(token));

            foreach (var typeToken in MatchTypeTokens(token as JObject))
            {
                yield return CreateMeasurementFromToken(typeToken);
            }
        }

        protected T EvalExpression<T>(JToken token, string name, bool isRequired = false, params TemplateExpression[] expressions)
        {
            EnsureArg.IsNotNull(token, nameof(token));
            EnsureArg.IsNotEmptyOrWhiteSpace(name, nameof(name));

            if (expressions?.Any(e => e != null) == true)
            {
                var exceptions = new List<Exception>();

                foreach (var expression in expressions)
                {
                    if (expression == null)
                    {
                        continue;
                    }

                    var evaluator = ExpressionEvaluatorFactory.Create(expression);
                    var evaluatedToken = evaluator.SelectToken(token);

                    if (evaluatedToken != null)
                    {
                        try
                        {
                            var value = evaluatedToken.Value<T>();
                            var isNull = value is string s ? string.IsNullOrWhiteSpace(s) : value == null;

                            if (isRequired && isNull)
                            {
                                exceptions.Add(new IncompatibleDataException($"A null or empty value was supplied for the required field [{name}]"));
                            }
                            else
                            {
                                return value;
                            }
                        }
                        catch (Exception e)
                        {
                            exceptions.Add(new IncompatibleDataException($"Encounted an error while extracting value for [{name}] using expression {expression.Value}", e));
                        }
                    }
                }

                if (isRequired)
                {
                    if (exceptions.Count > 0)
                    {
                        throw new IncompatibleDataException($"Unable to extract required value for [{name}]", new AggregateException(exceptions));
                    }

                    throw new IncompatibleDataException($"Unable to extract required value for [{name}] using {string.Join(",", expressions.Select(e => e.Value).ToArray())}");
                }
            }
            else if (isRequired)
            {
                throw new IncompatibleDataException($"An expression must be set for [{name}]");
            }

            return default;
        }

        protected static bool IsExpressionDefined(params TemplateExpression[] expressions)
        {
            EnsureArg.IsNotNull(expressions, nameof(expressions));

            return expressions.Any(ex => ex != null && !string.IsNullOrWhiteSpace(ex.Value));
        }

        protected virtual DateTime? GetTimestamp(JToken token) => EvalExpression<DateTime?>(token, nameof(Template.TimestampExpression), true, Template.TimestampExpression);

        protected virtual string GetDeviceId(JToken token) => EvalExpression<string>(token, nameof(Template.DeviceIdExpression), true, Template.DeviceIdExpression);

        protected virtual string GetPatientId(JToken token) => EvalExpression<string>(token, nameof(Template.PatientIdExpression), false, Template.PatientIdExpression);

        protected virtual string GetEncounterId(JToken token) => EvalExpression<string>(token, nameof(Template.EncounterIdExpression), false, Template.EncounterIdExpression);

        protected virtual string GetCorrelationId(JToken token) => EvalExpression<string>(token, nameof(Template.CorrelationIdExpression), true, Template.CorrelationIdExpression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IEnumerable<JToken> MatchTypeTokens(JObject token)
        {
            EnsureArg.IsNotNull(token, nameof(token));
            var evaluator = CreateRequiredExpressionEvaluator(Template.TypeMatchExpression, nameof(Template.TypeMatchExpression));

            foreach (var extractedToken in evaluator.SelectTokens(token))
            {
                // Add the extracted data as an element of the original data.
                // This allows subsequent expressions access to data from the original event data

                var tokenClone = token.DeepClone() as JObject;
                tokenClone.Add(MatchedToken, extractedToken);
                yield return tokenClone;
            }
        }

        protected IExpressionEvaluator CreateRequiredExpressionEvaluator(TemplateExpression expression, string expressionName)
        {
            EnsureArg.IsNotNullOrWhiteSpace(expressionName, nameof(expressionName));

            // If the expression object or its value aren't set, throw a detailed exception
            if (string.IsNullOrWhiteSpace(expression?.Value))
            {
                throw new IncompatibleDataException($"An expression must be set for [{expressionName}]");
            }

            return ExpressionEvaluatorFactory.Create(Template.TypeMatchExpression);
        }

        private Measurement CreateMeasurementFromToken(JToken token)
        {
            // Current assumption is that the expressions should match a single element and will error otherwise.

            string deviceId = GetDeviceId(token);
            DateTime? timestamp = GetTimestamp(token);

            string correlationId = null;
            if (IsExpressionDefined(Template.CorrelationIdExpression))
            {
                correlationId = GetCorrelationId(token);
            }

            var measurement = new Measurement
            {
                DeviceId = deviceId,
                OccurrenceTimeUtc = timestamp.Value,
                Type = Template.TypeName,
                PatientId = GetPatientId(token),
                EncounterId = GetEncounterId(token),
                CorrelationId = correlationId,
            };

            if (Template.Values != null)
            {
                foreach (var val in Template.Values)
                {
                    var value = EvalExpression<string>(token, val.ValueName, val.Required, val.ValueExpression);
                    if (value != null)
                    {
                        measurement.Properties.Add(new MeasurementProperty { Name = val.ValueName, Value = value });
                    }
                }
            }

            return measurement;
        }
    }
}
