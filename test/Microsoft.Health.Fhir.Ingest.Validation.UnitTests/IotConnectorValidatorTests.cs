﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json.Linq;
using Model = Hl7.Fhir.Model;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Validation.UnitTests
{
    public class IotConnectorValidatorTests
    {
        private CollectionTemplateFactory<IContentTemplate, IContentTemplate> _collectionTemplateFactory;
        private ITemplateFactory<string, ITemplateContext<ILookupTemplate<IFhirTemplate>>> _fhirTemplateFactory;
        private IotConnectorValidator _iotConnectorValidator;

        public IotConnectorValidatorTests()
        {
            _fhirTemplateFactory = CollectionFhirTemplateFactory.Default;
            _collectionTemplateFactory = new CollectionContentTemplateFactory(
                new JsonPathContentTemplateFactory(),
                new IotJsonPathContentTemplateFactory(),
                new IotCentralJsonPathContentTemplateFactory());

            _iotConnectorValidator = new IotConnectorValidator(
                _collectionTemplateFactory,
                _fhirTemplateFactory,
                new R4FhirLookupTemplateProcessor());
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateHrAndBloodPressureValid.json", @"TestInput/data_CollectionFhirTemplateValid.json")]
        public void Given_ValidMappingFiles_And_No_DeviceMapping_No_Exceptions_Or_Warnings_Found(string deviceMapping, string fhirMapping)
        {
            var result = _iotConnectorValidator.PerformValidation(null, deviceMapping, fhirMapping);
            Assert.Empty(result.Exceptions);
            Assert.Empty(result.Warnings);
            Assert.Empty(result.Measurements);
            Assert.Null(result.DeviceEvent);
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
            Assert.Empty(result.Exceptions);
            Assert.Empty(result.Warnings);
            Assert.Collection(result.Measurements, m =>
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
            Assert.Collection(result.Observations, o =>
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
            Assert.NotNull(result.DeviceEvent);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateInvalid.json", @"TestInput/data_CodeValueFhirTemplateInvalid_MissingFields.json")]
        public void Given_InValidMappingFiles_Exceptions_Found(string deviceMapping, string fhirMapping)
        {
            // [0]"Validation errors:\nFailed to deserialize the JsonPathContentTemplate content: \n  Required property 'DeviceIdExpression' not found in JSON. \n  Required property 'TimestampExpression' not found in JSON. \nFailed to deserialize the IotJsonPathContentTemplate content: \n  Required property 'TypeMatchExpression' not found in JSON. "   string
            // "Validation errors:\nExpected TemplateType value CollectionFhirTemplate, actual CodeValueFhir."
            var result = _iotConnectorValidator.PerformValidation(null, deviceMapping, fhirMapping);
            Assert.Collection(
                result.Exceptions,
                (error) => error.Contains("Required property 'DeviceIdExpression' not found in JSON"),
                (error) => error.Contains("Expected TemplateType value CollectionFhirTemplate, actual CodeValueFhir"));
            Assert.Empty(result.Measurements);
            Assert.Null(result.DeviceEvent);
        }
    }
}
