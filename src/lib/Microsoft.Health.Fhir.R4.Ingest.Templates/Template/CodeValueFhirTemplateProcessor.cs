// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.R4.Ingest.Templates.Extensions;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CodeValueFhirTemplateProcessor : FhirTemplateProcessor<CodeValueFhirTemplate, Observation>
    {
        private readonly IFhirValueProcessor<IObservationData, DataType> _valueProcessor;

        public CodeValueFhirTemplateProcessor()
            : this(new R4FhirValueProcessor())
        {
        }

        public CodeValueFhirTemplateProcessor(IFhirValueProcessor<IObservationData, DataType> valueProcessor)
        {
            EnsureArg.IsNotNull(valueProcessor);
            _valueProcessor = valueProcessor;
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

        protected static IObservationData CreateMergeData((DateTime start, DateTime end) dataPeriod, (DateTime start, DateTime end) observationPeriod, IEnumerable<(DateTime, string)> data)
        {
            return new ObservationData
            {
                DataPeriod = dataPeriod,
                ObservationPeriod = observationPeriod,
                Data = EnsureArg.IsNotNull(data, nameof(data)),
            };
        }

        protected static (DateTime start, DateTime end) GetObservationPeriod(Observation observation)
        {
            EnsureArg.IsNotNull(observation, nameof(observation));
            var effective = EnsureArg.IsNotNull(observation.Effective, nameof(observation.Effective));

            switch (effective)
            {
                case FhirDateTime dt:
                    return (dt.ToUtcDateTime(), dt.ToUtcDateTime());
                case Period p:
                    return p.ToUtcDateTimePeriod();
                default:
                    throw new NotSupportedException($"Observation effective type of {effective.GetType()} is not supported.");
            }
        }

        protected override Observation CreateObservationImpl(CodeValueFhirTemplate template, IObservationGroup grp)
        {
            EnsureArg.IsNotNull(template, nameof(template));
            EnsureArg.IsNotNull(grp, nameof(grp));

            var observation = new Observation
            {
                Status = ObservationStatus.Final,
                Code = ResolveCode(grp.Name, template.Codes),
                Issued = DateTimeOffset.UtcNow,
                Effective = grp.Boundary.ToPeriod(),
            };

            if (template?.Category?.Count > 0)
            {
                observation.Category = ResolveCategory(template.Category);
            }

            var values = grp.GetValues();

            if (!string.IsNullOrWhiteSpace(template?.Value?.ValueName) && values.TryGetValue(template?.Value?.ValueName, out var obValues))
            {
                observation.Value = _valueProcessor.CreateValue(template.Value, CreateMergeData(grp.Boundary, grp.Boundary, obValues));
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
                                Value = _valueProcessor.CreateValue(component.Value, CreateMergeData(grp.Boundary, grp.Boundary, compValues)),
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

            var mergedObservation = existingObservation.DeepCopy() as Observation;

            mergedObservation.Category = null;
            if (template?.Category?.Count > 0)
            {
                mergedObservation.Category = ResolveCategory(template.Category);
            }

            var values = grp.GetValues();
            (DateTime start, DateTime end) observationPeriod = GetObservationPeriod(mergedObservation);

            // Update observation value
            if (!string.IsNullOrWhiteSpace(template?.Value?.ValueName) && values.TryGetValue(template?.Value?.ValueName, out var obValues))
            {
                mergedObservation.Value = _valueProcessor.MergeValue(template.Value, CreateMergeData(grp.Boundary, observationPeriod, obValues), mergedObservation.Value);
            }

            // Update observation component values
            if (template?.Components?.Count > 0)
            {
                if (mergedObservation.Component == null)
                {
                    mergedObservation.Component = new List<Observation.ComponentComponent>(template.Components.Count);
                }

                foreach (var component in template.Components)
                {
                    if (values.TryGetValue(component.Value.ValueName, out var compValues))
                    {
                        var foundComponent = mergedObservation.Component
                            .Where(c => c.Code.Coding.Any(code => code.Code == component.Value.ValueName && code.System == FhirImportService.ServiceSystem))
                            .FirstOrDefault();

                        if (foundComponent == null)
                        {
                            mergedObservation.Component.Add(
                                new Observation.ComponentComponent
                                {
                                    Code = ResolveCode(component.Value.ValueName, component.Codes),
                                    Value = _valueProcessor.CreateValue(component.Value, CreateMergeData(grp.Boundary, observationPeriod, compValues)),
                                });
                        }
                        else
                        {
                          foundComponent.Value = _valueProcessor.MergeValue(component.Value, CreateMergeData(grp.Boundary, observationPeriod, compValues), foundComponent.Value);
                        }
                    }
                }
            }

            // Update observation effective period if merge values exist outside the current period.
            if (grp.Boundary.Start < observationPeriod.start)
            {
                observationPeriod.start = grp.Boundary.Start;
            }

            if (grp.Boundary.End > observationPeriod.end)
            {
                observationPeriod.end = grp.Boundary.End;
            }

            mergedObservation.Effective = observationPeriod.ToPeriod();

            return mergedObservation;
        }

        protected override IEnumerable<IObservationGroup> CreateObservationGroupsImpl(CodeValueFhirTemplate template, IMeasurementGroup measurementGroup)
        {
            EnsureArg.IsNotNull(template, nameof(template));

            IObservationGroupFactory<IMeasurementGroup> factory = new MeasurementObservationGroupFactory(template.PeriodInterval);
            return factory.Build(measurementGroup);
        }
    }
}
