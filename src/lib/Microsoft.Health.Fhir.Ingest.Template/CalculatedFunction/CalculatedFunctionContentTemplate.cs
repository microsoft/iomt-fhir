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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public class CalculatedFunctionContentTemplate : IContentTemplate
    {
        public const string MatchedToken = "matchedToken";

        [JsonProperty(Required = Required.Always)]
        public virtual string TypeName { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression TypeMatchExpression { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression DeviceIdExpression { get; set; }

        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression PatientIdExpression { get; set; }

        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression EncounterIdExpression { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression TimestampExpression { get; set; }

        [JsonConverter(typeof(ExpressionJsonConverter))]
        public virtual Expression CorrelationIdExpression { get; set; }

#pragma warning disable CA2227
        public virtual IList<CalculatedFunctionValueExpression> Values { get; set; }
#pragma warning restore CA2227

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ExpressionLanguage DefaultExpressionLanguage { get; set; } = ExpressionLanguage.JsonPath;

        [JsonIgnore]
        public virtual IExpressionEvaluatorFactory ExpressionEvaluatorFactory { get; set; }

        public virtual IEnumerable<Measurement> GetMeasurements(JToken token)
        {
            EnsureArg.IsNotNull(token, nameof(token));
            EnsureArg.IsOfType(token, typeof(JObject), nameof(token));

            foreach (var typeToken in MatchTypeTokens(token as JObject))
            {
                yield return CreateMeasurementFromToken(typeToken);
            }
        }

        protected T EvalExpression<T>(JToken token, string name, Expression expression, bool isRequired = false)
        {
            EnsureArg.IsNotNull(token, nameof(token));

            if (expression != null && !string.IsNullOrWhiteSpace(expression.Value))
            {
                EnsureArg.IsNotNull(expression?.Value, nameof(expression.Value));

                var evaluator = ExpressionEvaluatorFactory.Create(expression);
                var evaluatedToken = evaluator.SelectToken(token);

                if (evaluatedToken != null)
                {
                    return evaluatedToken.Value<T>();
                }
                else if (isRequired)
                {
                    throw new InvalidOperationException($"Unable to extract required value for [{name}] using expression {expression.Value}");
                }
            }
            else if (isRequired)
            {
                throw new InvalidOperationException($"An expression must be set for [{name}]");
            }

            return default;
        }

        protected static bool IsExpressionDefined(params Expression[] expressions)
        {
            EnsureArg.IsNotNull(expressions, nameof(expressions));

            return expressions.Any(ex => ex != null && !string.IsNullOrWhiteSpace(ex.Value));
        }

        protected virtual DateTime? GetTimestamp(JToken token) => EvalExpression<DateTime?>(token, nameof(TimestampExpression), TimestampExpression, true);

        protected virtual string GetDeviceId(JToken token) => EvalExpression<string>(token, nameof(DeviceIdExpression), DeviceIdExpression, true);

        protected virtual string GetPatientId(JToken token) => EvalExpression<string>(token, nameof(PatientIdExpression), PatientIdExpression);

        protected virtual string GetEncounterId(JToken token) => EvalExpression<string>(token, nameof(EncounterIdExpression), EncounterIdExpression);

        protected virtual string GetCorrelationId(JToken token) => EvalExpression<string>(token, nameof(CorrelationIdExpression), CorrelationIdExpression, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<JToken> MatchTypeTokens(JObject token)
        {
            EnsureArg.IsNotNull(token, nameof(token));
            var evaluator = ExpressionEvaluatorFactory.Create(TypeMatchExpression);

            foreach (var extractedToken in evaluator.SelectTokens(token))
            {
                // Add the extracted data as an element of the original data.
                // This allows subsequent expressions access to data from the original event data

                var tokenClone = token.DeepClone() as JObject;
                tokenClone.Add(MatchedToken, extractedToken);
                yield return tokenClone;
            }
        }

        private Measurement CreateMeasurementFromToken(JToken token)
        {
            // Current assumption is that the expressions should match a single element and will error otherwise.

            string deviceId = GetDeviceId(token);
            DateTime? timestamp = GetTimestamp(token);

            string correlationId = null;
            if (IsExpressionDefined(CorrelationIdExpression))
            {
                correlationId = GetCorrelationId(token);
            }

            var measurement = new Measurement
            {
                DeviceId = deviceId,
                OccurrenceTimeUtc = timestamp.Value,
                Type = TypeName,
                PatientId = GetPatientId(token),
                EncounterId = GetEncounterId(token),
                CorrelationId = correlationId,
            };

            if (Values != null)
            {
                for (var i = 0; i < Values.Count; i++)
                {
                    var val = Values[i];
                    var value = EvalExpression<string>(token, val.ValueName, val, val.Required);
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
