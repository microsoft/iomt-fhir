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
    public class JsonPathContentTemplate : IContentTemplate
    {
        public virtual string TypeName { get; set; }

        public virtual string TypeMatchExpression { get; set; }

        public virtual string DeviceIdExpression { get; set; }

        public virtual string PatientIdExpression { get; set; }

        public virtual string EncounterIdExpression { get; set; }

        public virtual string TimestampExpression { get; set; }

        public virtual string CorrelationIdExpression { get; set; }

#pragma warning disable CA2227
        public virtual IList<JsonPathValueExpression> Values { get; set; }
#pragma warning restore CA2227

        public virtual IEnumerable<Measurement> GetMeasurements(JToken token)
        {
            EnsureArg.IsNotNull(token, nameof(token));

            foreach (var typeToken in MatchTypeTokens(token))
            {
                yield return CreateMeasurementFromToken(typeToken);
            }
        }

        protected static T EvalExpression<T>(JToken token, params string[] expressions)
        {
            EnsureArg.IsNotNull(token, nameof(token));
            EnsureArg.IsNotNull(expressions, nameof(expressions));

            if (expressions == null)
            {
                return default;
            }

            foreach (var expression in expressions)
            {
                if (string.IsNullOrWhiteSpace(expression))
                {
                    continue;
                }

                var evaluatedToken = token.SelectToken(expression);

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

        protected virtual DateTime? GetTimestamp(JToken token) => EvalExpression<DateTime?>(token, TimestampExpression);

        protected virtual string GetDeviceId(JToken token) => EvalExpression<string>(token, DeviceIdExpression);

        protected virtual string GetPatientId(JToken token) => EvalExpression<string>(token, PatientIdExpression);

        protected virtual string GetEncounterId(JToken token) => EvalExpression<string>(token, EncounterIdExpression);

        protected virtual string GetCorrelationId(JToken token) => EvalExpression<string>(token, CorrelationIdExpression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<JToken> MatchTypeTokens(JToken token)
        {
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
