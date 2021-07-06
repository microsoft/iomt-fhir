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
            EnsureArg.IsTrue(token is JObject);

            foreach (var typeToken in MatchTypeTokens(token as JObject))
            {
                yield return CreateMeasurementFromToken(typeToken);
            }
        }

        protected T EvalExpression<T>(JToken token, params Expression[] expressions)
        {
            EnsureArg.IsNotNull(token, nameof(token));
            EnsureArg.IsNotNull(expressions, nameof(expressions));

            if (expressions == null)
            {
                return default;
            }

            foreach (var expression in expressions)
            {
                if (string.IsNullOrWhiteSpace(expression?.Value))
                {
                    continue;
                }

                var evaluator = ExpressionEvaluatorFactory.Create(expression);

                var evaluatedToken = evaluator.SelectToken(token);

                if (evaluatedToken == null)
                {
                    continue;
                }

                return evaluatedToken.Value<T>();
            }

            return default;
        }

        protected static bool IsExpressionDefined(params Expression[] expressions)
        {
            EnsureArg.IsNotNull(expressions, nameof(expressions));

            return expressions.Any(ex => ex != null && !string.IsNullOrWhiteSpace(ex.Value));
        }

        protected virtual DateTime? GetTimestamp(JToken token) => EvalExpression<DateTime?>(token, TimestampExpression);

        protected virtual string GetDeviceId(JToken token) => EvalExpression<string>(token, DeviceIdExpression);

        protected virtual string GetPatientId(JToken token) => EvalExpression<string>(token, PatientIdExpression);

        protected virtual string GetEncounterId(JToken token) => EvalExpression<string>(token, EncounterIdExpression);

        protected virtual string GetCorrelationId(JToken token) => EvalExpression<string>(token, CorrelationIdExpression);

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
            EnsureArg.IsNotNull(deviceId, nameof(deviceId));

            DateTime? timestamp = GetTimestamp(token);
            EnsureArg.IsNotNull(timestamp, nameof(timestamp));

            string correlationId = null;
            if (IsExpressionDefined(CorrelationIdExpression))
            {
                correlationId = GetCorrelationId(token);
                EnsureArg.IsNotNull(correlationId, nameof(correlationId));
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
                    var value = EvalExpression<string>(token, val);
                    if (value != null)
                    {
                        measurement.Properties.Add(new MeasurementProperty { Name = val.ValueName, Value = value });
                    }
                    else
                    {
                        if (val.Required)
                        {
                            throw new InvalidOperationException($"Required value {val.ValueName} not found.");
                        }
                    }
                }
            }

            return measurement;
        }
    }
}
