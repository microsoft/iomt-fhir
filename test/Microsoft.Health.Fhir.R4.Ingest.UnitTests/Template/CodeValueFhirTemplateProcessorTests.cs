// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeValueFhirTemplateProcessorTests
    {
        [Fact]
        public void GivenDefaultCtor_WhenCtor_ThenProcessorCreated_Test()
        {
            var processor = new CodeValueFhirTemplateProcessor();
            Assert.NotNull(processor);
        }

        [Fact]
        public void GivenTemplate_WhenCreateObservationGroups_ThenPeriodIntervalCorrectlyUsed_Test()
        {
            var valueProcessor = Substitute.For<IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>>();
            var template = Substitute.For<CodeValueFhirTemplate>().Mock(m => m.PeriodInterval.Returns(ObservationPeriodInterval.Single));
            var measurementGroup = new MeasurementGroup
            {
                Data = new List<Measurement>
                {
                    new Measurement { OccurrenceTimeUtc = DateTime.UtcNow },
                },
            };

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var result = processor.CreateObservationGroups(template, measurementGroup);
            Assert.Single(result);

            _ = template.Received(1).PeriodInterval;
        }

        [Fact]
        public void GivenEmptyTemplate_WhenCreateObservation_ThenShellObservationReturned_Test()
        {
            var valueProcessor = Substitute.For<IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>>();

            var template = Substitute.For<CodeValueFhirTemplate>();

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p1", new[] { (DateTime.UtcNow, "v1") } },
            };

            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.Name.Returns("code"))
                .Mock(m => m.GetValues().Returns(values));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var observation = processor.CreateObservation(template, observationGroup);
            Assert.NotNull(observation);

            Assert.Equal("code", observation.Code.Text);
            Assert.Collection(
                observation.Code.Coding,
                c =>
                {
                    Assert.Equal(FhirImportService.ServiceSystem, c.System);
                    Assert.Equal("code", c.Code);
                    Assert.Equal("code", c.Display);
                });
            Assert.Equal(ObservationStatus.Final, observation.Status);
            Assert.NotNull(observation.Issued);
            var period = observation.Effective as Period;
            Assert.NotNull(period);
            Assert.Equal(boundary.start, period.StartElement.ToDateTimeOffset(TimeSpan.Zero).UtcDateTime);
            Assert.Equal(boundary.end, period.EndElement.ToDateTimeOffset(TimeSpan.Zero).UtcDateTime);

            valueProcessor.DidNotReceiveWithAnyArgs().CreateValue(default, default);
        }

        [Fact]
        public void GivenTemplateWithValue_WhenCreateObservation_ThenObservationReturned_Test()
        {
            var element = Substitute.For<Element>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>>()
                .Mock(m => m.CreateValue(null, default).ReturnsForAnyArgs(element));

            var valueType = Substitute.For<FhirValueType>()
                .Mock(m => m.ValueName.Returns("p1"));
            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Codes.Returns(
                    new List<FhirCode>
                    {
                        new FhirCode { Code = "code", Display = "a", System = "b" },
                    }))
                .Mock(m => m.Value.Returns(valueType));

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p1", new[] { (DateTime.UtcNow, "v1") } },
            };

            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.Name.Returns("code"))
                .Mock(m => m.GetValues().Returns(values));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var observation = processor.CreateObservation(template, observationGroup);
            Assert.NotNull(observation);

            Assert.Equal("code", observation.Code.Text);
            Assert.Collection(
                observation.Code.Coding,
                c =>
                {
                    Assert.Equal("b", c.System);
                    Assert.Equal("code", c.Code);
                    Assert.Equal("a", c.Display);
                },
                c =>
                {
                    Assert.Equal(FhirImportService.ServiceSystem, c.System);
                    Assert.Equal("code", c.Code);
                    Assert.Equal("code", c.Display);
                });
            Assert.Equal(ObservationStatus.Final, observation.Status);
            Assert.NotNull(observation.Issued);
            var period = observation.Effective as Period;
            Assert.NotNull(period);
            Assert.Equal(boundary.start, period.StartElement.ToDateTimeOffset(TimeSpan.Zero).UtcDateTime);
            Assert.Equal(boundary.end, period.EndElement.ToDateTimeOffset(TimeSpan.Zero).UtcDateTime);
            Assert.Equal(element, observation.Value);

            valueProcessor.Received(1)
                .CreateValue(
                    valueType,
                    Arg.Is<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values)>(
                        v => v.start == boundary.start
                        && v.end == boundary.end
                        && v.values.First().Item2 == "v1"));
        }

        [Fact]
        public void GivenTemplateWithComponent_WhenCreateObservation_ThenObservationReturned_Test()
        {
            var element = Substitute.For<Element>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>>()
                .Mock(m => m.CreateValue(null, default).ReturnsForAnyArgs(element));

            var valueType = Substitute.For<FhirValueType>()
                .Mock(m => m.ValueName.Returns("p2"));
            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Codes.Returns(
                    new List<FhirCode>
                    {
                        new FhirCode { Code = "code1", Display = "a", System = "b" },
                    }))
                .Mock(m => m.Components.Returns(
                    new List<CodeValueMapping>
                    {
                        new CodeValueMapping
                        {
                          Codes = new List<FhirCode>
                          {
                              new FhirCode { Code = "code2", Display = "c", System = "d" },
                          },
                          Value = valueType,
                        },
                    }));

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p1", new[] { (DateTime.UtcNow, "v1") } },
                { "p2", new[] { (DateTime.UtcNow, "v2") } },
            };

            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.Name.Returns("code"))
                .Mock(m => m.GetValues().Returns(values));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var observation = processor.CreateObservation(template, observationGroup);
            Assert.NotNull(observation);

            Assert.Equal("code", observation.Code.Text);
            Assert.Collection(
                observation.Code.Coding,
                c =>
                {
                    Assert.Equal("b", c.System);
                    Assert.Equal("code1", c.Code);
                    Assert.Equal("a", c.Display);
                },
                c =>
                {
                    Assert.Equal(FhirImportService.ServiceSystem, c.System);
                    Assert.Equal("code", c.Code);
                    Assert.Equal("code", c.Display);
                });
            Assert.Equal(ObservationStatus.Final, observation.Status);
            Assert.NotNull(observation.Issued);
            var period = observation.Effective as Period;
            Assert.NotNull(period);
            Assert.Equal(boundary.start, period.StartElement.ToDateTimeOffset(TimeSpan.Zero).UtcDateTime);
            Assert.Equal(boundary.end, period.EndElement.ToDateTimeOffset(TimeSpan.Zero).UtcDateTime);
            Assert.Null(observation.Value);
            Assert.Collection(
                observation.Component,
                c =>
                {
                    Assert.Collection(
                        c.Code.Coding,
                        code =>
                        {
                            Assert.Equal("d", code.System);
                            Assert.Equal("code2", code.Code);
                            Assert.Equal("c", code.Display);
                        },
                        code =>
                        {
                            Assert.Equal(FhirImportService.ServiceSystem, code.System);
                            Assert.Equal("p2", code.Code);
                            Assert.Equal("p2", code.Display);
                        });
                    Assert.Equal(element, c.Value);
                });

            valueProcessor.Received(1)
                .CreateValue(
                    valueType,
                    Arg.Is<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values)>(
                        v => v.start == boundary.start
                        && v.end == boundary.end
                        && v.values.First().Item2 == "v2"));
        }

        [Fact]
        public void GivenEmptyTemplate_WhenMergObservation_ThenObservationReturned_Test()
        {
            var oldObservation = new Observation();

            var valueProcessor = Substitute.For<IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>>();

            var template = Substitute.For<CodeValueFhirTemplate>();

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p1", new[] { (DateTime.UtcNow, "v1") } },
            };

            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.Name.Returns("code"))
                .Mock(m => m.GetValues().Returns(values));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);

            Assert.Equal(ObservationStatus.Amended, newObservation.Status);
            valueProcessor.DidNotReceiveWithAnyArgs().MergeValue(default, default, default);
        }

        [Fact]
        public void GivenTemplateWithValue_WhenMergObservation_ThenObservationReturned_Test()
        {
            Element oldValue = new Quantity();
            var oldObservation = new Observation
            {
                Value = oldValue,
            };

            var element = Substitute.For<Element>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>>()
                .Mock(m => m.MergeValue(default, default, default).ReturnsForAnyArgs(element));

            var valueType = Substitute.For<FhirValueType>()
               .Mock(m => m.ValueName.Returns("p1"));
            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Codes.Returns(
                    new List<FhirCode>
                    {
                        new FhirCode { Code = "code", Display = "a", System = "b" },
                    }))
                .Mock(m => m.Value.Returns(valueType));

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p1", new[] { (DateTime.UtcNow, "v1") } },
            };

            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.Name.Returns("code"))
                .Mock(m => m.GetValues().Returns(values));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);
            Assert.Equal(element, newObservation.Value);

            // oldObservation.Value
            Assert.Equal(ObservationStatus.Amended, newObservation.Status);
            valueProcessor.Received(1)
                .MergeValue(
                    valueType,
                    Arg.Is<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values)>(
                        v => v.start == boundary.start
                        && v.end == boundary.end
                        && v.values.First().Item2 == "v1"),
                    oldValue);
        }

        [Fact]
        public void GivenTemplateWithComponent_WhenMergObservation_ThenObservationReturned_Test()
        {
            Element oldValue = new Quantity();
            var oldObservation = new Observation
            {
                Component = new List<Observation.ComponentComponent>
                {
                    new Observation.ComponentComponent
                    {
                        Code = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Display = "p2",
                                    System = FhirImportService.ServiceSystem,
                                    Code = "p2",
                                },
                            },
                        },
                        Value = oldValue,
                    },
                },
            };

            var element = Substitute.For<Element>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>>()
                .Mock(m => m.MergeValue(default, default, default).ReturnsForAnyArgs(element))
                .Mock(m => m.CreateValue(null, default).ReturnsForAnyArgs(element));

            var valueType1 = Substitute.For<FhirValueType>()
                .Mock(m => m.ValueName.Returns("p2"));
            var valueType2 = Substitute.For<FhirValueType>()
                .Mock(m => m.ValueName.Returns("p3"));

            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Codes.Returns(
                    new List<FhirCode>
                    {
                        new FhirCode { Code = "code1", Display = "a", System = "b" },
                    }))
                .Mock(m => m.Components.Returns(
                    new List<CodeValueMapping>
                    {
                        new CodeValueMapping
                        {
                          Codes = new List<FhirCode>
                          {
                              new FhirCode { Code = "code2", Display = "c", System = "d" },
                          },
                          Value = valueType1,
                        },
                        new CodeValueMapping
                        {
                          Codes = new List<FhirCode>
                          {
                              new FhirCode { Code = "code3", Display = "e", System = "f" },
                          },
                          Value = valueType2,
                        },
                    }));

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p2", new[] { (DateTime.UtcNow, "v2") } },
                { "p3", new[] { (DateTime.UtcNow, "v3") } },
            };

            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.Name.Returns("code"))
                .Mock(m => m.GetValues().Returns(values));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);
            Assert.Null(newObservation.Value);

            Assert.Collection(
                newObservation.Component,
                c =>
                {
                    // Existing component value in observation that was merged
                    Assert.Equal(element, c.Value);
                },
                c =>
                {
                    // New component value added to observation
                    Assert.Collection(
                        c.Code.Coding,
                        code =>
                        {
                            Assert.Equal("f", code.System);
                            Assert.Equal("code3", code.Code);
                            Assert.Equal("e", code.Display);
                        },
                        code =>
                        {
                            Assert.Equal(FhirImportService.ServiceSystem, code.System);
                            Assert.Equal("p3", code.Code);
                            Assert.Equal("p3", code.Display);
                        });
                    Assert.Equal(element, c.Value);
                });

            Assert.Equal(ObservationStatus.Amended, newObservation.Status);
            valueProcessor.Received(1)
                .MergeValue(
                    valueType1,
                    Arg.Is<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values)>(
                        v => v.start == boundary.start
                        && v.end == boundary.end
                        && v.values.First().Item2 == "v2"),
                    oldValue);

            valueProcessor.Received(1)
                .CreateValue(
                    valueType2,
                    Arg.Is<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values)>(
                        v => v.start == boundary.start
                        && v.end == boundary.end
                        && v.values.First().Item2 == "v3"));
        }

        [Fact]
        public void GivenTemplateWithComponentAndObservationWithOutComponent_WhenMergObservation_ThenObservationWithComponentAddedReturned_Test()
        {
            Element oldValue = new Quantity();
            var oldObservation = new Observation();

            var element = Substitute.For<Element>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>>()
                .Mock(m => m.CreateValue(null, default).ReturnsForAnyArgs(element));

            var valueType1 = Substitute.For<FhirValueType>()
                .Mock(m => m.ValueName.Returns("p2"));

            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Codes.Returns(
                    new List<FhirCode>
                    {
                        new FhirCode { Code = "code1", Display = "a", System = "b" },
                    }))
                .Mock(m => m.Components.Returns(
                    new List<CodeValueMapping>
                    {
                        new CodeValueMapping
                        {
                          Codes = new List<FhirCode>
                          {
                              new FhirCode { Code = "code2", Display = "c", System = "d" },
                          },
                          Value = valueType1,
                        },
                    }));

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p2", new[] { (DateTime.UtcNow, "v2") } },
            };

            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.Name.Returns("code"))
                .Mock(m => m.GetValues().Returns(values));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);
            Assert.Null(newObservation.Value);

            Assert.Collection(
                newObservation.Component,
                c =>
                {
                    // New component value added to observation
                    Assert.Collection(
                        c.Code.Coding,
                        code =>
                        {
                            Assert.Equal("d", code.System);
                            Assert.Equal("code2", code.Code);
                            Assert.Equal("c", code.Display);
                        },
                        code =>
                        {
                            Assert.Equal(FhirImportService.ServiceSystem, code.System);
                            Assert.Equal("p2", code.Code);
                            Assert.Equal("p2", code.Display);
                        });
                    Assert.Equal(element, c.Value);
                });

            Assert.Equal(ObservationStatus.Amended, newObservation.Status);

            valueProcessor.Received(1)
                .CreateValue(
                    valueType1,
                    Arg.Is<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values)>(
                        v => v.start == boundary.start
                        && v.end == boundary.end
                        && v.values.First().Item2 == "v2"));
        }
    }
}
