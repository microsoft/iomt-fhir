// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnsureThat;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeValueFhirTemplateProcessor : FhirTemplateProcessor<CodeValueFhirTemplate, Observation>
    {
        private readonly IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element> _valueProcessor;

        public CodeValueFhirTemplateProcessor()
            : this(new R4FhirValueProcessor())
        {
        }

        public CodeValueFhirTemplateProcessor(IFhirValueProcessor<(DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element> valueProcessor)
        {
            EnsureArg.IsNotNull(valueProcessor);
            _valueProcessor = valueProcessor;
        }

        protected override Observation CreateObseravtionImpl(CodeValueFhirTemplate template, IObservationGroup grp)
        {
            EnsureArg.IsNotNull(template, nameof(template));
            EnsureArg.IsNotNull(grp, nameof(grp));

            var observation = new Observation
            {
                Status = ObservationStatus.Final,
                Code = ResolveCode(grp.Name, template.Codes),
                Issued = DateTimeOffset.UtcNow,
                Effective = new Period
                {
                    Start = grp.Boundary.Start.ToString("o", CultureInfo.InvariantCulture.DateTimeFormat),
                    End = grp.Boundary.End.ToString("o", CultureInfo.InvariantCulture.DateTimeFormat),
                },
            };

            if (template?.Category?.Count > 0)
            {
                observation.Category = ResolveCategory(template.Category);
            }

            var values = grp.GetValues();

            if (!string.IsNullOrWhiteSpace(template?.Value?.ValueName) && values.TryGetValue(template?.Value?.ValueName, out var obValues))
            {
                observation.Value = _valueProcessor.CreateValue(template.Value, (grp.Boundary.Start, grp.Boundary.End, obValues));
            }

            if (template?.Components?.Count > 0)
            {
                observation.Component = new List<Observation.ComponentComponent>(template.Components.Count);

                foreach (var component in template.Components)
                {
                    if (values.TryGetValue(component.Value.ValueName, out var compValues))
                    {
                        observation.Component.Add(
                            new Observation.ComponentComponent
                            {
                                Code = ResolveCode(component.Value.ValueName, component.Codes),
                                Value = _valueProcessor.CreateValue(component.Value, (grp.Boundary.Start, grp.Boundary.End, compValues)),
                            });
                    }
                }
            }

            return observation;
        }

        protected override Observation MergeObservationImpl(CodeValueFhirTemplate template, IObservationGroup grp, Observation existingObservation)
        {
            EnsureArg.IsNotNull(grp, nameof(grp));
            EnsureArg.IsNotNull(existingObservation, nameof(existingObservation));

            existingObservation.Status = ObservationStatus.Amended;

            existingObservation.Category = null;
            if (template?.Category?.Count > 0)
            {
                existingObservation.Category = ResolveCategory(template.Category);
            }

            var values = grp.GetValues();

            if (!string.IsNullOrWhiteSpace(template?.Value?.ValueName) && values.TryGetValue(template?.Value?.ValueName, out var obValues))
            {
                existingObservation.Value = _valueProcessor.MergeValue(template.Value, (grp.Boundary.Start, grp.Boundary.End, obValues), existingObservation.Value);
            }

            if (template?.Components?.Count > 0)
            {
                if (existingObservation.Component == null)
                {
                    existingObservation.Component = new List<Observation.ComponentComponent>(template.Components.Count);
                }

                foreach (var component in template.Components)
                {
                    if (values.TryGetValue(component.Value.ValueName, out var compValues))
                    {
                        var foundComponent = existingObservation.Component
                            .Where(c => c.Code.Coding.Any(code => code.Code == component.Value.ValueName && code.System == FhirImportService.ServiceSystem))
                            .FirstOrDefault();

                        if (foundComponent == null)
                        {
                            existingObservation.Component.Add(
                                new Observation.ComponentComponent
                                {
                                    Code = ResolveCode(component.Value.ValueName, component.Codes),
                                    Value = _valueProcessor.CreateValue(component.Value, (grp.Boundary.Start, grp.Boundary.End, compValues)),
                                });
                        }
                        else
                        {
                          foundComponent.Value = _valueProcessor.MergeValue(component.Value, (grp.Boundary.Start, grp.Boundary.End, compValues), foundComponent.Value);
                        }
                    }
                }
            }

            return existingObservation;
        }

        protected override IEnumerable<IObservationGroup> CreateObservationGroupsImpl(CodeValueFhirTemplate template, IMeasurementGroup measurementGroup)
        {
            EnsureArg.IsNotNull(template, nameof(template));

            IObservationGroupFactory<IMeasurementGroup> factory = new MeasurementObservationGroupFactory(template.PeriodInterval);
            return factory.Build(measurementGroup);
        }

        protected static CodeableConcept ResolveCode(string type, IList<FhirCode> codes)
        {
            return new CodeableConcept
            {
                Text = type,
                Coding = codes.Select(c => new Coding { System = c.System, Code = c.Code, Display = c.Display })
                .Append(new Coding { System = FhirImportService.ServiceSystem, Code = type, Display = type })
                .ToList(),
            };
        }

        protected static List<CodeableConcept> ResolveCategory(IList<FhirCodeableConcept> categories)
        {
            return categories.Select(category =>
                new CodeableConcept
                {
                    Text = category.Text,
                    Coding = category?.Codes?.Select(code => new Coding { System = code.System, Code = code.Code, Display = code.Display })
                    .ToList(),
                }).ToList();
        }
    }
}
