// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests.Samples;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template.Generator.UnitTests
{
    public class CalculatedContentTemplateGeneratorTests
    {
        private readonly ITemplateGenerator<TestModel> _templateGenerator;

        public CalculatedContentTemplateGeneratorTests()
        {
            _templateGenerator = new TestCalculatedContentTemplateGenerator();
        }

        [Theory]
        [FileData(@"TestInput/deviceData_HeartRate.json", @"Expected/deviceData_HeartRate.json")]
        [FileData(@"TestInput/deviceData_BloodPressure.json", @"Expected/deviceData_BloodPressure.json")]
        [FileData(@"TestInput/deviceData_OxygenSaturation.json", @"Expected/deviceData_OxygenSaturation.json")]
        public async Task GivenModel_WhenGenerateTemplateCalled_TemplateGenerated(string modelJson, string expectedJson)
        {
            TestModel model = JsonConvert.DeserializeObject<TestModel>(modelJson);
            JObject expected = JObject.Parse(expectedJson);

            JObject deviceData = await _templateGenerator.GenerateTemplate(model, CancellationToken.None);

            Assert.True(JToken.DeepEquals(expected, deviceData));
        }
    }
}
