// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests.Samples
{
    public class TestCalculatedContentTemplateGenerator : CalculatedContentTemplateGenerator<TestModel>
    {
        public override Task<TemplateExpression> GetDeviceIdExpression(string typeName, TestModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Device).ToLowerInvariant()}" });
        }

        public override Task<TemplateExpression> GetTimestampExpression(string typeName, TestModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Time).ToLowerInvariant()}" });
        }

        public override Task<TemplateExpression> GetTypeMatchExpression(string typeName, TestModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$..[?(@.{nameof(model.Type).ToLowerInvariant()}=='{model.Type}')]" });
        }

        public override Task<IEnumerable<string>> GetTypeNames(TestModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>(new List<string>() { model.Type });
        }

        public override Task<TemplateExpression> GetPatientIdExpression(string typeName, TestModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Patient).ToLowerInvariant()}" });
        }

        public override Task<TemplateExpression> GetEncounterIdExpression(string typeName, TestModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Encounter).ToLowerInvariant()}" });
        }

        public override Task<TemplateExpression> GetCorrelationIdExpression(string typeName, TestModel model, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TemplateExpression() { Value = $"$.{nameof(model.Correlation).ToLowerInvariant()}" });
        }

        public override Task<IList<CalculatedFunctionValueExpression>> GetValues(string typeName, TestModel model, CancellationToken cancellationToken)
        {
            var values = new List<CalculatedFunctionValueExpression>();

            foreach (var value in model.Values)
            {
                values.Add(new CalculatedFunctionValueExpression()
                {
                    ValueName = value.Key,
                    ValueExpression = new TemplateExpression() { Value = $"$.{nameof(model.Values).ToLowerInvariant()}.{value.Key}" },
                });
            }

            return Task.FromResult<IList<CalculatedFunctionValueExpression>>(values);
        }
    }
}
