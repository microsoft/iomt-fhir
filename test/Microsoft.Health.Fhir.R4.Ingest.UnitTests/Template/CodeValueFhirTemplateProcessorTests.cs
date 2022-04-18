// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.R4.Ingest.Templates.Extensions;
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
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>();
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
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>();

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
            var dataType = Substitute.For<DataType>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>()
                .Mock(m => m.CreateValue(null, default).ReturnsForAnyArgs(dataType));

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
            Assert.Equal(dataType, observation.Value);

            valueProcessor.Received(1)
                .CreateValue(
                    valueType,
                    Arg.Is<IObservationData>(
                         v => v.DataPeriod.start == boundary.start
                        && v.DataPeriod.end == boundary.end
                        && v.ObservationPeriod.start == boundary.start
                        && v.ObservationPeriod.end == boundary.end
                        && v.Data.First().Item2 == "v1"));
        }

        [Fact]
        public void GivenTemplateWithComponent_WhenCreateObservation_ThenObservationReturned_Test()
        {
            var dataType = Substitute.For<DataType>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>()
                .Mock(m => m.CreateValue(null, default).ReturnsForAnyArgs(dataType));

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
                    Assert.Equal(dataType, c.Value);
                });

            valueProcessor.Received(1)
                .CreateValue(
                    valueType,
                    Arg.Is<IObservationData>(
                        v => v.DataPeriod.start == boundary.start
                        && v.DataPeriod.end == boundary.end
                        && v.ObservationPeriod.start == boundary.start
                        && v.ObservationPeriod.end == boundary.end
                        && v.Data.First().Item2 == "v2"));
        }

        [Fact]
        public void GivenEmptyTemplate_WhenMergObservation_ThenObservationReturned_Test()
        {
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>();

            var template = Substitute.For<CodeValueFhirTemplate>();

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p1", new[] { (DateTime.UtcNow, "v1") } },
            };

            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            var oldObservation = new Observation
            {
                Effective = new Period
                {
                    Start = boundary.start.ToString("o", CultureInfo.InvariantCulture.DateTimeFormat),
                    End = boundary.end.ToString("o", CultureInfo.InvariantCulture.DateTimeFormat),
                },
            };

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.Name.Returns("code"))
                .Mock(m => m.GetValues().Returns(values));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);

            Assert.Equal(oldObservation.Status, newObservation.Status);
            valueProcessor.DidNotReceiveWithAnyArgs().MergeValue(default, default, default);
        }

        [Fact]
        public void GivenTemplateWithValue_WhenMergObservation_ThenObservationReturned_Test()
        {
            DataType oldValue = new Quantity();

            var dataType = Substitute.For<DataType>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>()
                .Mock(m => m.MergeValue(default, default, default).ReturnsForAnyArgs(dataType));

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

            var oldObservation = new Observation
            {
                Effective = boundary.ToPeriod(),
                Value = oldValue,
            };

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);
            Assert.Equal(dataType, newObservation.Value);

            Assert.Equal(oldObservation.Status, newObservation.Status);
            valueProcessor.Received(1)
                .MergeValue(
                    valueType,
                    Arg.Is<IObservationData>(
                         v => v.DataPeriod.start == boundary.start
                        && v.DataPeriod.end == boundary.end
                        && v.ObservationPeriod.start == boundary.start
                        && v.ObservationPeriod.end == boundary.end
                        && v.Data.First().Item2 == "v1"),
                    Arg.Is<Quantity>(v => v.IsExactly(oldValue)));
        }

        [Fact]
        public void GivenTemplateWithComponent_WhenMergObservation_ThenObservationReturned_Test()
        {
            DataType oldValue = new Quantity();

            var dataType = Substitute.For<DataType>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>()
                .Mock(m => m.MergeValue(default, default, default).ReturnsForAnyArgs(dataType))
                .Mock(m => m.CreateValue(null, default).ReturnsForAnyArgs(dataType));

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

            var oldObservation = new Observation
            {
                Effective = boundary.ToPeriod(),
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

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);
            Assert.Null(newObservation.Value);

            Assert.Collection(
                newObservation.Component,
                c =>
                {
                    // Existing component value in observation that was merged
                    Assert.Equal(dataType, c.Value);
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
                    Assert.Equal(dataType, c.Value);
                });

            Assert.Equal(oldObservation.Status, newObservation.Status);
            valueProcessor.Received(1)
                .MergeValue(
                    valueType1,
                    Arg.Is<IObservationData>(
                         v => v.DataPeriod.start == boundary.start
                        && v.DataPeriod.end == boundary.end
                        && v.ObservationPeriod.start == boundary.start
                        && v.ObservationPeriod.end == boundary.end
                        && v.Data.First().Item2 == "v2"),
                    Arg.Is<Quantity>(v => v.IsExactly(oldValue)));

            valueProcessor.Received(1)
                .CreateValue(
                    valueType2,
                    Arg.Is<IObservationData>(
                         v => v.DataPeriod.start == boundary.start
                        && v.DataPeriod.end == boundary.end
                        && v.ObservationPeriod.start == boundary.start
                        && v.ObservationPeriod.end == boundary.end
                        && v.Data.First().Item2 == "v3"));
        }

        [Fact]
        public void GivenTemplateWithComponentAndObservationWithOutComponent_WhenMergObservation_ThenObservationWithComponentAddedReturned_Test()
        {
            DataType oldValue = new Quantity();

            var dataType = Substitute.For<DataType>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>()
                .Mock(m => m.CreateValue(null, default).ReturnsForAnyArgs(dataType));

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

            var oldObservation = new Observation
            {
                Effective = boundary.ToPeriod(),
            };

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
                    Assert.Equal(dataType, c.Value);
                });

            Assert.Equal(oldObservation.Status, newObservation.Status);

            valueProcessor.Received(1)
                .CreateValue(
                    valueType1,
                    Arg.Is<IObservationData>(
                         v => v.DataPeriod.start == boundary.start
                        && v.DataPeriod.end == boundary.end
                        && v.ObservationPeriod.start == boundary.start
                        && v.ObservationPeriod.end == boundary.end
                        && v.Data.First().Item2 == "v2"));
        }

        [Fact]
        public void GivenTemplateWithCategory_WhenCreateObservation_ThenCategoryReturned_Test()
        {
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>();
            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Category.Returns(
                    new List<FhirCodeableConcept>
                    {
                        new FhirCodeableConcept
                        {
                            Codes = new List<FhirCode>
                            {
                                new FhirCode { Code = "a", Display = "b", System = "c" },
                                new FhirCode { Code = "d", Display = "e", System = "f" },
                            },
                            Text = "category with two codes",
                        },
                        new FhirCodeableConcept
                        {
                            Codes = new List<FhirCode>
                            {
                                new FhirCode { Code = "y", System = "z" },
                            },
                            Text = "category with no display",
                        },
                        new FhirCodeableConcept
                        {
                            Text = "category with no codes",
                        },
                    }));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

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

            var observation = processor.CreateObservation(template, observationGroup);
            Assert.Collection(
                observation.Category,
                category =>
                {
                    Assert.Collection(
                        category.Coding,
                        code =>
                        {
                            Assert.Equal("a", code.Code);
                            Assert.Equal("b", code.Display);
                            Assert.Equal("c", code.System);
                        },
                        code =>
                        {
                            Assert.Equal("d", code.Code);
                            Assert.Equal("e", code.Display);
                            Assert.Equal("f", code.System);
                        });
                    Assert.Equal("category with two codes", category.Text);
                },
                category =>
                {
                    Assert.Collection(
                        category.Coding,
                        code =>
                        {
                            Assert.Equal("y", code.Code);
                            Assert.Null(code.Display);
                            Assert.Equal("z", code.System);
                        });
                    Assert.Equal("category with no display", category.Text);
                },
                category =>
                {
                    Assert.False(category.Coding.Any());
                    Assert.Equal("category with no codes", category.Text);
                });
        }

        [Fact]
        public void GivenTemplateWithCategory_WhenMergeObservationWithCategory_ThenCategoryReplaced_Test()
        {
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>();
            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Category.Returns(
                    new List<FhirCodeableConcept>
                    {
                        new FhirCodeableConcept
                        {
                            Codes = new List<FhirCode>
                            {
                                new FhirCode { Code = "new category code", Display = "new category display", System = "new category system" },
                            },
                            Text = "new category text",
                        },
                    }));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

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

            var oldObservation = new Observation()
            {
                Effective = boundary.ToPeriod(),
                Category = new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Display = "old category display",
                                    System = "old category system",
                                    Code = "old category code",
                                },
                            },
                        Text = "old category text",
                    },
                },
            };

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);

            Assert.Collection(
                newObservation.Category,
                category =>
                {
                    Assert.Collection(
                        category.Coding,
                        code =>
                        {
                            Assert.Equal("new category system", code.System);
                            Assert.Equal("new category code", code.Code);
                            Assert.Equal("new category display", code.Display);
                        });
                    Assert.Equal("new category text", category.Text);
                });
        }

        [Fact]
        public void GivenTemplateWithoutCategory_WhenMergeObservationWithCategory_ThenCategoryRemoved()
        {
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>();
            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Category.Returns(
                    new List<FhirCodeableConcept> { }));

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

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

            var oldObservation = new Observation()
            {
                Effective = boundary.ToPeriod(),
                Category = new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Display = "old category display",
                                    System = "old category system",
                                    Code = "old category code",
                                },
                            },
                        Text = "old category text",
                    },
                },
            };

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);

            Assert.False(newObservation.Category.Any());
        }

        [Fact]
        public void GivenExistingObservation_WhenMergeObservationWithDataOutsideEffectivePeriod_ThenPeriodUpdated_Test()
        {
            DataType oldValue = new Quantity();

            var dataType = Substitute.For<DataType>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>()
                .Mock(m => m.MergeValue(default, default, default).ReturnsForAnyArgs(dataType));

            var valueType = Substitute.For<FhirValueType>()
               .Mock(m => m.ValueName.Returns("p1"));
            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Value.Returns(valueType));

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p1", new[] { (DateTime.UtcNow, "v1") } },
            };

            DateTime observationStart = new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            DateTime observationEnd = new DateTime(2019, 1, 3, 0, 0, 0, DateTimeKind.Utc);
            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 4, 0, 0, 0, DateTimeKind.Utc));

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.GetValues().Returns(values));

            var oldObservation = new Observation
            {
                Effective = (observationStart, observationEnd).ToPeriod(),
                Value = oldValue,
            };

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);

            var (start, end) = Assert.IsType<Period>(newObservation.Effective)
                .ToUtcDateTimePeriod();
            Assert.Equal(boundary.start, start);
            Assert.Equal(boundary.end, end);

            Assert.Equal(oldObservation.Status, newObservation.Status);
            valueProcessor.Received(1)
                .MergeValue(
                    valueType,
                    Arg.Is<IObservationData>(
                         v => v.DataPeriod.start == boundary.start
                        && v.DataPeriod.end == boundary.end
                        && v.ObservationPeriod.start == observationStart
                        && v.ObservationPeriod.end == observationEnd
                        && v.Data.First().Item2 == "v1"),
                    Arg.Is<Quantity>(v => v.IsExactly(oldValue)));
        }

        [Fact]
        public void GivenExistingObservation_WhenMergeObservationWithDataInsideEffectivePeriod_ThenPeriodNotUpdated_Test()
        {
            DataType oldValue = new Quantity();

            var dataType = Substitute.For<DataType>();
            var valueProcessor = Substitute.For<IFhirValueProcessor<IObservationData, DataType>>()
                .Mock(m => m.MergeValue(default, default, default).ReturnsForAnyArgs(dataType));

            var valueType = Substitute.For<FhirValueType>()
               .Mock(m => m.ValueName.Returns("p1"));
            var template = Substitute.For<CodeValueFhirTemplate>()
                .Mock(m => m.Value.Returns(valueType));

            var values = new Dictionary<string, IEnumerable<(DateTime, string)>>
            {
                { "p1", new[] { (DateTime.UtcNow, "v1") } },
            };

            DateTime observationStart = new DateTime(2018, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            DateTime observationEnd = new DateTime(2019, 1, 3, 0, 0, 0, DateTimeKind.Utc);
            (DateTime start, DateTime end) boundary = (new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            var observationGroup = Substitute.For<IObservationGroup>()
                .Mock(m => m.Boundary.Returns(boundary))
                .Mock(m => m.GetValues().Returns(values));

            var oldObservation = new Observation
            {
                Effective = (observationStart, observationEnd).ToPeriod(),
                Value = oldValue,
            };

            var processor = new CodeValueFhirTemplateProcessor(valueProcessor);

            var newObservation = processor.MergeObservation(template, observationGroup, oldObservation);

            var (start, end) = Assert.IsType<Period>(newObservation.Effective)
                .ToUtcDateTimePeriod();
            Assert.Equal(observationStart, start);
            Assert.Equal(observationEnd, end);

            Assert.Equal(oldObservation.Status, newObservation.Status);
            valueProcessor.Received(1)
                .MergeValue(
                    valueType,
                    Arg.Is<IObservationData>(
                         v => v.DataPeriod.start == boundary.start
                        && v.DataPeriod.end == boundary.end
                        && v.ObservationPeriod.start == observationStart
                        && v.ObservationPeriod.end == observationEnd
                        && v.Data.First().Item2 == "v1"),
                    Arg.Is<Quantity>(v => v.IsExactly(oldValue)));
        }
    }
}
