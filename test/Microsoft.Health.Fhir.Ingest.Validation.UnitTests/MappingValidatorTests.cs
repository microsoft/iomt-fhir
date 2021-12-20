// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Fhir.Ingest.Validation.Extensions;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Validation.UnitTests
{
    public class MappingValidatorTests
    {
        private CollectionTemplateFactory<IContentTemplate, IContentTemplate> _collectionTemplateFactory;
        private ITemplateFactory<string, ITemplateContext<ILookupTemplate<IFhirTemplate>>> _fhirTemplateFactory;
        private MappingValidator _iotConnectorValidator;

        public MappingValidatorTests()
        {
            _fhirTemplateFactory = CollectionFhirTemplateFactory.Default;
            _collectionTemplateFactory = new CollectionContentTemplateFactory(
                new JsonPathContentTemplateFactory(),
                new IotJsonPathContentTemplateFactory(),
                new IotCentralJsonPathContentTemplateFactory());

            _iotConnectorValidator = new MappingValidator(
                _collectionTemplateFactory,
                _fhirTemplateFactory,
                new R4FhirLookupTemplateProcessor());
        }

        [Fact]
        public void When_No_MappingFilesAreProvided_Exception_Is_Thrown()
        {
            Assert.Throws<ArgumentException>(() => _iotConnectorValidator.PerformValidation(null, null, null));
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndBloodPressureValid.json", @"TestInput/data_CollectionFhirTemplateValid.json")]
        public void Given_ValidMappingFiles_And_No_DeviceMapping_No_Exceptions_Or_Warnings_Found(string deviceMapping, string fhirMapping)
        {
            var result = _iotConnectorValidator.PerformValidation(null, deviceMapping, fhirMapping);
            Assert.Empty(result.TemplateResult.Exceptions);
            Assert.Empty(result.DeviceResults);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndBloodPressureValid.json", @"TestInput/data_CollectionFhirTemplateValid.json")]
        public void Given_ValidMappingFiles_And_Valid_DeviceMapping_No_Exceptions_And_MeasurementsAreCreated(string deviceMapping, string fhirMapping)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new
            {
                systolic = "60",
                diastolic = "80",
                device = "abc",
                date = time,
                session = "abcdefg",
                patient = "patient123",
            });

            var result = _iotConnectorValidator.PerformValidation(token, deviceMapping, fhirMapping);
            Assert.Empty(result.TemplateResult.Exceptions);
            Assert.Collection(result.DeviceResults, d =>
            {
                Assert.Collection(d.Measurements, m =>
                {
                    Assert.Equal("bloodpressure", m.Type);
                    Assert.Equal(time, m.OccurrenceTimeUtc);
                    Assert.Equal("abc", m.DeviceId);
                    Assert.Equal("patient123", m.PatientId);
                    Assert.Null(m.CorrelationId);
                    Assert.Null(m.EncounterId);
                    Assert.Collection(
                        m.Properties,
                        p =>
                        {
                            Assert.Equal("systolic", p.Name);
                            Assert.Equal("60", p.Value);
                        },
                        p =>
                        {
                            Assert.Equal("diastolic", p.Name);
                            Assert.Equal("80", p.Value);
                        });
                });
            });

            Assert.Collection(result.DeviceResults, d =>
            {
                Assert.Collection(d.Observations, o =>
                {
                    Assert.Equal("bloodpressure", o.Code.Text);
                    Assert.Collection(
                        o.Component,
                        c =>
                        {
                            Assert.Equal("diastolic", c.Code.Text);
                            Assert.Contains("80", (c.Value as Model.SampledData).Data);
                        },
                        c =>
                        {
                            Assert.Equal("systolic", c.Code.Text);
                            Assert.Contains("60", (c.Value as Model.SampledData).Data);
                        });
                });
            });
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateInvalid.json")]
        public void When_Only_InvalidDeviceMapping_Provided_Only_DeviceMapping_Exceptions_Logged(string deviceMapping)
        {
            var result = _iotConnectorValidator.PerformValidation(null, deviceMapping, null);
            Assert.Collection(
                result.TemplateResult.GetErrors(Models.ErrorLevel.ERROR),
                (error) => error.Message.Contains("Required property 'DeviceIdExpression' not found in JSON"));
            Assert.Empty(result.DeviceResults);
        }

        [Theory]
        [FileData(@"TestInput/data_CodeValueFhirTemplateInvalid_MissingFields.json")]
        public void When_Only_InvalidFhirMapping_Provided_Only_FhirMapping_Exceptions_Logged(string fhirMapping)
        {
            var result = _iotConnectorValidator.PerformValidation(null, null, fhirMapping);
            Assert.Collection(
                result.TemplateResult.GetErrors(Models.ErrorLevel.ERROR),
                (error) => error.Message.Contains("Expected TemplateType value CollectionFhirTemplate, actual CodeValueFhir"));
            Assert.Empty(result.DeviceResults);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateInvalid.json", @"TestInput/data_CodeValueFhirTemplateInvalid_MissingFields.json")]
        public void Given_InvalidMappingFiles_Exceptions_Found(string deviceMapping, string fhirMapping)
        {
            // [0]"Validation errors:\nFailed to deserialize the JsonPathContentTemplate content: \n  Required property 'DeviceIdExpression' not found in JSON. \n  Required property 'TimestampExpression' not found in JSON. \nFailed to deserialize the IotJsonPathContentTemplate content: \n  Required property 'TypeMatchExpression' not found in JSON. "   string
            // "Validation errors:\nExpected TemplateType value CollectionFhirTemplate, actual CodeValueFhir."
            var result = _iotConnectorValidator.PerformValidation(null, deviceMapping, fhirMapping);
            Assert.Collection(
                result.TemplateResult.GetErrors(Models.ErrorLevel.ERROR),
                (error) => error.Message.Contains("Required property 'DeviceIdExpression' not found in JSON"),
                (error) => error.Message.Contains("Expected TemplateType value CollectionFhirTemplate, actual CodeValueFhir"));
            Assert.Empty(result.DeviceResults);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndBloodPressureValid.json", @"TestInput/data_CollectionFhirTemplateMissingTypeInvalid.json")]
        public void Given_ValidDeviceMapping_And_FhirMapping_WithMissingTypeName_Warnings_Found(string deviceMapping, string fhirMapping)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new
            {
                systolic = "60",
                diastolic = "80",
                device = "abc",
                date = time,
                session = "abcdefg",
                patient = "patient123",
            });
            var result = _iotConnectorValidator.PerformValidation(token, deviceMapping, fhirMapping);
            Assert.Empty(result.TemplateResult.GetErrors(Models.ErrorLevel.ERROR));
            Assert.Collection(
                result.TemplateResult.GetErrors(Models.ErrorLevel.WARN),
                (error) => error.Message.Contains("No matching Fhir Template exists for Device Mapping [bloodpressure]"));
            Assert.Collection(result.DeviceResults, d =>
            {
                Assert.Collection(
                    d.GetErrors(Models.ErrorLevel.ERROR),
                    e => e.Message.StartsWith("No Fhir Template exists with the type name [bloodpressure]"));
                Assert.Single(d.Measurements);
                Assert.NotNull(d.DeviceEvent);
                Assert.Empty(d.Observations);
                Assert.Empty(d.GetErrors(Models.ErrorLevel.WARN));
            });
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndBloodPressureValid.json", @"TestInput/data_CollectionFhirTemplateIncorrectValueNameInvalid.json")]
        public void Given_ValidDeviceMapping_And_FhirMapping_WithIncorrectValueName_Warnings_Found(string deviceMapping, string fhirMapping)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new
            {
                systolic = "60",
                diastolic = "80",
                device = "abc",
                date = time,
                session = "abcdefg",
                patient = "patient123",
            });

            var result = _iotConnectorValidator.PerformValidation(new List<JToken>() { token, token }, deviceMapping, fhirMapping);

            Assert.Empty(result.TemplateResult.GetErrors(Models.ErrorLevel.ERROR));
            Assert.Collection(
               result.TemplateResult.GetErrors(Models.ErrorLevel.WARN),
               (error) => error.Message.StartsWith("The value [systolic] in Device Mapping [bloodpressure] is not represented within the Fhir Template"));

            Action<Models.DeviceResult> deviceResultAction = (d) =>
            {
                Assert.Single(d.Measurements);
                Assert.NotNull(d.DeviceEvent);
                Assert.Empty(d.GetErrors(Models.ErrorLevel.WARN));
                Assert.Empty(d.Exceptions);
                Assert.Collection(d.Observations, o =>
                {
                    Assert.Equal("bloodpressure", o.Code.Text);
                    Assert.Collection(
                        o.Component,
                        c =>
                        {
                            Assert.Equal("diastolic", c.Code.Text);
                            Assert.Contains("80", (c.Value as Model.SampledData).Data);
                        });
                });
            };

            Assert.Collection(result.DeviceResults, deviceResultAction, deviceResultAction);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndBloodPressureValid.json", @"TestInput/data_CollectionFhirTemplateValid.json")]
        public void Given_ValidMappingFiles_And_Valid_DeviceMapping_NoMeasurementsAreCreated_With_Bad_DeviceData(string deviceMapping, string fhirMapping)
        {
            var time = DateTime.UtcNow;
            var token = JToken.FromObject(new
            {
                device = "abc",
                date = time,
                session = "abcdefg",
                patient = "patient123",
            });

            var result = _iotConnectorValidator.PerformValidation(token, deviceMapping, fhirMapping);
            Assert.Empty(result.TemplateResult.Exceptions);
            Assert.Collection(result.DeviceResults, d =>
            {
                Assert.Collection(
                  d.GetErrors(Models.ErrorLevel.WARN),
                  (error) => error.Message.StartsWith("No measurements were produced"));
                Assert.Empty(d.Measurements);
                Assert.Empty(d.Observations);
                Assert.NotNull(d.DeviceEvent);
                Assert.Empty(d.GetErrors(Models.ErrorLevel.ERROR));
            });
        }
    }
}
