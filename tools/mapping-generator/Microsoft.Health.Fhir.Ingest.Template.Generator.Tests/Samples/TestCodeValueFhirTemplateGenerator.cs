// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests.Samples
{
    public class TestCodeValueFhirTemplateGenerator : CodeValueFhirTemplateGenerator<CalculatedFunctionContentTemplate>
    {
        public override Task<IList<FhirCode>> GetCodes(CalculatedFunctionContentTemplate model, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetCodes(model.TypeName));
        }

        public override Task<IList<FhirCodeableConcept>> GetCategory(CalculatedFunctionContentTemplate model, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetCategory("Vital Signs"));
        }

        public override Task<FhirValueType> GetValue(CalculatedFunctionContentTemplate model, CancellationToken cancellationToken)
        {
            if (model.Values.Count == 1)
            {
                return Task.FromResult(GetValue(model.Values[0]));
            }

            return Task.FromResult<FhirValueType>(null);
        }

        public override Task<IList<CodeValueMapping>> GetComponents(CalculatedFunctionContentTemplate model, CancellationToken cancellationToken)
        {
            if (model.Values.Count > 1)
            {
                IList<CodeValueMapping> components = new List<CodeValueMapping>();

                foreach (var value in model.Values)
                {
                    components.Add(new CodeValueMapping()
                    {
                        Codes = GetCodes(value.ValueName),
                        Value = GetValue(value),
                    });
                }

                return Task.FromResult(components);
            }

            return Task.FromResult<IList<CodeValueMapping>>(null);
        }

        private IList<FhirCode> GetCodes(string name)
        {
            return JsonConvert.DeserializeObject<IList<FhirCode>>(GetData($"Codes/{name}.json"));
        }

        private IList<FhirCodeableConcept> GetCategory(string name)
        {
            return new List<FhirCodeableConcept>()
            {
                new FhirCodeableConcept()
                {
                    Text = name,
                    Codes = GetCodes(name),
                },
            };
        }

        private FhirValueType GetValue(CalculatedFunctionValueExpression expression)
        {
            FhirValueType value = JsonConvert.DeserializeObject<FhirValueType>(GetData($"Values/{expression.ValueName}.json"));
            value.ValueName = expression.ValueName;

            return value;
        }

        private string GetData(string filePath)
        {
            filePath = filePath.Replace(" ", string.Empty);

            var path = Path.IsPathRooted(filePath) ? filePath : Path.Combine(Directory.GetCurrentDirectory(), filePath);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File {path} not found.");
            }

            return File.ReadAllText(path);
        }
    }
}
