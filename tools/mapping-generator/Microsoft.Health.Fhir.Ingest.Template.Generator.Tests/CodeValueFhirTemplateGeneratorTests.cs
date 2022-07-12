// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests.Samples;
using Microsoft.Health.Fhir.Ingest.Template.Serialization;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests
{
    public class CodeValueFhirTemplateGeneratorTests
    {
        private readonly ITemplateGenerator<TestModel> _deviceDataTemplateGenerator;
        private readonly ITemplateGenerator<CalculatedFunctionContentTemplate> _fhirMappingTemplateGenerator;

        public CodeValueFhirTemplateGeneratorTests()
        {
            _deviceDataTemplateGenerator = new TestCalculatedContentTemplateGenerator();
            _fhirMappingTemplateGenerator = new TestCodeValueFhirTemplateGenerator();
        }

        [Theory]
        [FileData(@"TestInput/deviceData_HeartRate.json", @"Expected/fhirMapping_HeartRate.json")]
        [FileData(@"TestInput/deviceData_BloodPressure.json", @"Expected/fhirMapping_BloodPressure.json")]
        [FileData(@"TestInput/deviceData_OxygenSaturation.json", @"Expected/fhirMapping_OxygenSaturation.json")]
        public async Task GivenModel_WhenGenerateTemplateCalled_TemplateGenerated(string modelJson, string expectedJson)
        {
            TestModel model = JsonConvert.DeserializeObject<TestModel>(modelJson);
            JObject expected = JObject.Parse(expectedJson);

            JArray deviceData = await _deviceDataTemplateGenerator.GenerateTemplates(model, CancellationToken.None);
            var deviceTemplate = deviceData[0]["template"].ToObject<CalculatedFunctionContentTemplate>();

            JArray fhirMappings = await _fhirMappingTemplateGenerator.GenerateTemplates(deviceTemplate, CancellationToken.None);

            Assert.True(JToken.DeepEquals(expected, fhirMappings.FirstOrDefault()));
        }
    }
}
