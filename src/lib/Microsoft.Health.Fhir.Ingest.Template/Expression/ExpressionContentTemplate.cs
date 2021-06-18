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
using Microsoft.Health.Fhir.Ingest.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template.Expression
{
    public class ExpressionContentTemplate : IContentTemplate
    {
        [JsonProperty(Required = Required.Always)]
        public virtual string TypeName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public virtual string TypeMatchExpression { get; set; }

        public virtual ExpressionLanguage? TypeMatchExpressionLanguage { get; set; }

        [JsonProperty(Required = Required.Always)]
        public virtual string DeviceIdExpression { get; set; }

        public virtual ExpressionLanguage? DeviceIdExpressionLanguage { get; set; }

        public virtual string PatientIdExpression { get; set; }

        public virtual ExpressionLanguage? PatientIdExpressionLanguage { get; set; }

        public virtual string EncounterIdExpression { get; set; }

        public virtual ExpressionLanguage? EncounterIdExpressionLanguage { get; set; }

        [JsonProperty(Required = Required.Always)]
        public virtual string TimestampExpression { get; set; }

        public virtual ExpressionLanguage? TimestampExpressionLanguage { get; set; }

        public virtual string CorrelationIdExpression { get; set; }

        public virtual ExpressionLanguage? CorrelationIdExpressionLanguage { get; set; }

#pragma warning disable CA2227
        public virtual IList<ExpressionValueExpression> Values { get; set; }
#pragma warning restore CA2227

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ExpressionLanguage DefaultExpressionLanguage { get; set; } = ExpressionLanguage.JsonPath;

        [JsonIgnore]
        public virtual IExpressionEvaluatorFactory ExpressionEvaluatorFactory { get; set; }

        public virtual IEnumerable<Measurement> GetMeasurements(JToken token)
        {
            EnsureArg.IsNotNull(token, nameof(token));

            foreach (var typeToken in MatchTypeTokens(token))
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
                if (string.IsNullOrWhiteSpace(expression.Value))
                {
                    continue;
                }

                var evaluator = ExpressionEvaluatorFactory.Create(expression);

                var evaluatedToken = evaluator.Evaluate(token);

                if (evaluatedToken == null)
                {
                    continue;
                }

                return evaluatedToken.Value<T>();
            }

            return default;
        }

        protected static bool IsExpressionDefined(params string[] expressions)
        {
            EnsureArg.IsNotNull(expressions, nameof(expressions));

            return expressions.Any(ex => !string.IsNullOrWhiteSpace(ex));
        }

        protected virtual DateTime? GetTimestamp(JToken token) => EvalExpression<DateTime?>(
            token,
            new Expression()
            {
                Id = nameof(TimestampExpression),
                Value = TimestampExpression,
                Language = TimestampExpressionLanguage ?? DefaultExpressionLanguage,
            });

        protected virtual string GetDeviceId(JToken token) => EvalExpression<string>(
            token,
            new Expression()
            {
                Id = nameof(DeviceIdExpression),
                Value = DeviceIdExpression,
                Language = DeviceIdExpressionLanguage ?? DefaultExpressionLanguage,
            });

        protected virtual string GetPatientId(JToken token) => EvalExpression<string>(
            token,
            new Expression()
            {
                Id = nameof(PatientIdExpression),
                Value = PatientIdExpression,
                Language = PatientIdExpressionLanguage ?? DefaultExpressionLanguage,
            });

        protected virtual string GetEncounterId(JToken token) => EvalExpression<string>(
            token,
            new Expression()
            {
                Id = nameof(EncounterIdExpression),
                Value = EncounterIdExpression,
                Language = EncounterIdExpressionLanguage ?? DefaultExpressionLanguage,
            });

        protected virtual string GetCorrelationId(JToken token) => EvalExpression<string>(
            token,
            new Expression()
            {
                Id = nameof(CorrelationIdExpression),
                Value = CorrelationIdExpression,
                Language = CorrelationIdExpressionLanguage ?? DefaultExpressionLanguage,
            });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<JToken> MatchTypeTokens(JToken token)
        {
            // TODO: Gather the matched tokens. Create a new JToken which contains the original root plus the extracted token. Return this token
            return token.SelectTokens(TypeMatchExpression);
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
                foreach (var val in Values)
                {
                    var value = token.SelectToken(val.ValueExpression);
                    if (value != null)
                    {
                        measurement.Properties.Add(new MeasurementProperty { Name = val.ValueName, Value = value.Value<string>() });
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
