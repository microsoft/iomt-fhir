// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeValueFhirTemplateFactoryTests
    {
        [Theory]
        [FileData(@"TestInput/data_CodeValueFhirTemplate_SampledData.json")]
        public void GivenValidTemplateJsonWithValueSampledDataType_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new CodeValueFhirTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var codeValueTemplate = template as CodeValueFhirTemplate;
            Assert.NotNull(codeValueTemplate);

            Assert.Equal("heartrate", codeValueTemplate.TypeName);
            Assert.Equal(ObservationPeriodInterval.Hourly, codeValueTemplate.PeriodInterval);
            Assert.NotNull(codeValueTemplate.Value);

            var value = codeValueTemplate.Value as SampledDataFhirValueType;
            Assert.NotNull(value);
            Assert.Equal(5000m, value.DefaultPeriod);
            Assert.Equal("bpm", value.Unit);
            Assert.Equal("hr", value.ValueName);

            Assert.Collection(
                codeValueTemplate.Codes,
                c =>
                {
                    Assert.Equal("8867-4", c.Code);
                    Assert.Equal("http://loinc.org", c.System);
                    Assert.Equal("Heart rate", c.Display);
                });
        }

        [Theory]
        [FileData(@"TestInput/data_CodeValueFhirTemplate_Components.json")]
        public void GivenValidTemplateJsonWithComponentValueSampledDataType_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new CodeValueFhirTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var codeValueTemplate = template as CodeValueFhirTemplate;
            Assert.NotNull(codeValueTemplate);

            Assert.Equal("bp", codeValueTemplate.TypeName);
            Assert.Equal(ObservationPeriodInterval.Hourly, codeValueTemplate.PeriodInterval);
            Assert.Null(codeValueTemplate.Value);

            Assert.Collection(
                codeValueTemplate.Components,
                c =>
                {
                    Assert.Collection(
                        c.Codes,
                        code =>
                        {
                            Assert.Equal("8867-4", code.Code);
                            Assert.Equal("Diastolic blood pressure", code.Display);
                            Assert.Equal("http://loinc.org", code.System);
                        });
                    var value = c.Value as SampledDataFhirValueType;
                    Assert.NotNull(value);
                    Assert.Equal(5000m, value.DefaultPeriod);
                    Assert.Equal("mmHg", value.Unit);
                    Assert.Equal("diastolic", value.ValueName);
                },
                c =>
                {
                    Assert.Collection(
                        c.Codes,
                        code =>
                        {
                            Assert.Equal("8480-6", code.Code);
                            Assert.Equal("Systolic blood pressure", code.Display);
                            Assert.Equal("http://loinc.org", code.System);
                        });
                    var value = c.Value as SampledDataFhirValueType;
                    Assert.NotNull(value);
                    Assert.Equal(5000m, value.DefaultPeriod);
                    Assert.Equal("mmHg", value.Unit);
                    Assert.Equal("systolic", value.ValueName);
                });
        }

        [Theory]
        [FileData(@"TestInput/data_CodeValueFhirTemplate_CodeableConceptData.json")]
        public void GivenValidTemplateJsonWithCodeableConceptDataType_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new CodeValueFhirTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var codeValueTemplate = template as CodeValueFhirTemplate;
            Assert.NotNull(codeValueTemplate);

            Assert.Equal("alarmevent", codeValueTemplate.TypeName);
            Assert.Equal(ObservationPeriodInterval.Single, codeValueTemplate.PeriodInterval);
            Assert.NotNull(codeValueTemplate.Value);

            var value = codeValueTemplate.Value as CodeableConceptFhirValueType;
            Assert.NotNull(value);
            Assert.Equal("alarm", value.ValueName);
            Assert.Equal("Alarm!", value.Text);
            Assert.Collection(
                value.Codes,
                c =>
                {
                    Assert.Equal("alarmevent", c.Code);
                    Assert.Equal("https://www.contso.com/events/v1", c.System);
                    Assert.Equal("Alarm Event", c.Display);
                });

            Assert.Collection(
                codeValueTemplate.Codes,
                c =>
                {
                    Assert.Equal("deviceevent", c.Code);
                    Assert.Equal("https://www.contso.com/events/v1", c.System);
                    Assert.Equal("Device Event", c.Display);
                });
        }

        [Theory]
        [FileData(@"TestInput/data_CodeValueFhirTemplate_Quantity.json")]
        public void GivenValidTemplateJsonWithComponentValueQuantityType_WhenFactoryCreate_ThenTemplateCreated_Test(string json)
        {
            var templateContainer = JsonConvert.DeserializeObject<TemplateContainer>(json);

            var factory = new CodeValueFhirTemplateFactory();

            var template = factory.Create(templateContainer);
            Assert.NotNull(template);

            var codeValueTemplate = template as CodeValueFhirTemplate;
            Assert.NotNull(codeValueTemplate);

            Assert.Equal("bp", codeValueTemplate.TypeName);
            Assert.Equal(ObservationPeriodInterval.Hourly, codeValueTemplate.PeriodInterval);
            Assert.Null(codeValueTemplate.Value);

            Assert.Collection(
                codeValueTemplate.Components,
                c =>
                {
                    Assert.Collection(
                        c.Codes,
                        code =>
                        {
                            Assert.Equal("8867-4", code.Code);
                            Assert.Equal("Diastolic blood pressure", code.Display);
                            Assert.Equal("http://loinc.org", code.System);
                        });
                    var value = c.Value as QuantityFhirValueType;
                    Assert.NotNull(value);
                    Assert.Equal("http://unitsofmeasure.org", value.System);
                    Assert.Equal("mm[Hg]", value.Code);
                    Assert.Equal("mmHg", value.Unit);
                    Assert.Equal("diastolic", value.ValueName);
                },
                c =>
                {
                    Assert.Collection(
                        c.Codes,
                        code =>
                        {
                            Assert.Equal("8480-6", code.Code);
                            Assert.Equal("Systolic blood pressure", code.Display);
                            Assert.Equal("http://loinc.org", code.System);
                        });
                    var value = c.Value as QuantityFhirValueType;
                    Assert.NotNull(value);
                    Assert.Equal("http://unitsofmeasure.org", value.System);
                    Assert.Equal("mm[Hg]", value.Code);
                    Assert.Equal("mmHg", value.Unit);
                    Assert.Equal("systolic", value.ValueName);
                });
        }

        [Fact]
        public void GivenInvalidTemplateTargetType_WhenFactoryCreate_ThenInvalidTemplateExceptionThrown_Test()
        {
            var templateContainer = new TemplateContainer();

            var factory = new CodeValueFhirTemplateFactory();

            var ex = Assert.Throws<InvalidTemplateException>(() => factory.Create(templateContainer));
            Assert.NotNull(ex);
        }

        [Fact]
        public void GivenInvalidTemplateBody_WhenFactoryCreate_ThenInvalidTemplateExceptionThrown_Test()
        {
            var templateContainer = new TemplateContainer
            {
                TemplateType = "CodeValueFhirTemplate",
                Template = null,
            };

            var factory = new CodeValueFhirTemplateFactory();

            var ex = Assert.Throws<InvalidTemplateException>(() => factory.Create(templateContainer));
            Assert.NotNull(ex);
        }
    }
}
