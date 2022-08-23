// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Common.Extension;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests.Samples
{
    public class TestProjectionCalculatedContentTemplateGenerator : CalculatedContentTemplateGenerator<TestModelProjection>
    {
        private readonly IDictionary<string, string> _typeMatchExpressionMap = new Dictionary<string, string>()
        {
            { "Blood Pressure", $"$..[?(@{nameof(TestModelProjection.Diastolic).ToLowerInvariant()} && @{nameof(TestModelProjection.Systolic).ToLowerInvariant()})]" },
            { "Heart Rate", $"$..[?(@{nameof(TestModelProjection.HeartRate).ToLowercaseFirstLetterVariant()})]" },
            { "Oxygen Saturation", $"$..[?(@{nameof(TestModelProjection.OxygenSaturation).ToLowercaseFirstLetterVariant()})]" },
        };

        private readonly IDictionary<string, IList<string>> _valuesMap = new Dictionary<string, IList<string>>()
        {
            { "Blood Pressure", new List<string>() { $"$.{nameof(TestModelProjection.Diastolic).ToLowerInvariant()}", $"$.{nameof(TestModelProjection.Systolic).ToLowerInvariant()}" } },
            { "Heart Rate", new List<string>() { $"$.{nameof(TestModelProjection.HeartRate).ToLowercaseFirstLetterVariant()}", } },
            { "Oxygen Saturation", new List<string>() { $"$.{nameof(TestModelProjection.OxygenSaturation).ToLowercaseFirstLetterVariant()}" } },
        };

        private readonly IDictionary<string, string> _valueNameMap = new Dictionary<string, string>()
        {
            { $"$.{nameof(TestModelProjection.Diastolic).ToLowerInvariant()}", nameof(TestModelProjection.Diastolic).ToLowerInvariant() },
            { $"$.{nameof(TestModelProjection.Systolic).ToLowerInvariant()}", nameof(TestModelProjection.Systolic).ToLowerInvariant() },
            { $"$.{nameof(TestModelProjection.HeartRate).ToLowercaseFirstLetterVariant()}", "beatsPerMinute" },
            { $"$.{nameof(TestModelProjection.OxygenSaturation).ToLowercaseFirstLetterVariant()}", "oxygenPercentage" },
        };

        public override Task<TemplateExpression> GetDeviceIdExpression(string typeName, TestModelProjection model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Device).ToLowerInvariant()}" });
        }

        public override Task<TemplateExpression> GetTimestampExpression(string typeName, TestModelProjection model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Time).ToLowerInvariant()}" });
        }

        public override Task<TemplateExpression> GetTypeMatchExpression(string typeName, TestModelProjection model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = _typeMatchExpressionMap[typeName] });
        }

        public override Task<IEnumerable<string>> GetTypeNames(TestModelProjection model, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>(_typeMatchExpressionMap.Keys);
        }

        public override Task<TemplateExpression> GetPatientIdExpression(string typeName, TestModelProjection model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Patient).ToLowerInvariant()}" });
        }

        public override Task<TemplateExpression> GetEncounterIdExpression(string typeName, TestModelProjection model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Encounter).ToLowerInvariant()}" });
        }

        public override Task<TemplateExpression> GetCorrelationIdExpression(string typeName, TestModelProjection model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Correlation).ToLowerInvariant()}" });
        }

        public override Task<IList<CalculatedFunctionValueExpression>> GetValues(string typeName, TestModelProjection model, CancellationToken cancellationToken)
        {
            var values = new List<CalculatedFunctionValueExpression>();

            foreach (var value in _valuesMap[typeName])
            {
                values.Add(new CalculatedFunctionValueExpression()
                {
                    ValueName = _valueNameMap[value],
                    ValueExpression = new TemplateExpression() { Value = value },
                });
            }

            return Task.FromResult<IList<CalculatedFunctionValueExpression>>(values);
        }
    }
}
