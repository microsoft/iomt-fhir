// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public abstract class FhirLookupTemplateProcessor<TObservation> : IFhirTemplateProcessor<ILookupTemplate<IFhirTemplate>, TObservation>
    {
        private static readonly Type SupportedType = typeof(ILookupTemplate<IFhirTemplate>);

        private readonly IDictionary<Type, IFhirTemplateProcessor<IFhirTemplate, TObservation>> _registeredTemplateProcessors;

        protected FhirLookupTemplateProcessor(params IFhirTemplateProcessor<IFhirTemplate, TObservation>[] processors)
        {
            _registeredTemplateProcessors = EnsureArg.HasItems(processors, nameof(processors))
                .ToDictionary(p => p.SupportedTemplateType);
        }

        public Type SupportedTemplateType => SupportedType;

        public TObservation CreateObservation(ILookupTemplate<IFhirTemplate> lookup, IObservationGroup observationGroup)
        {
            EnsureArg.IsNotNull(observationGroup, nameof(observationGroup));

            var (template, processor) = GetTemplateAndProcessor(observationGroup.Name, lookup);
            return processor.CreateObservation(template, observationGroup);
        }

        public IEnumerable<IObservationGroup> CreateObservationGroups(ILookupTemplate<IFhirTemplate> lookup, IMeasurementGroup measurementGroup)
        {
            EnsureArg.IsNotNull(measurementGroup, nameof(measurementGroup));

            var (template, processor) = GetTemplateAndProcessor(measurementGroup.MeasureType, lookup);
            return processor.CreateObservationGroups(template, measurementGroup);
        }

        public TObservation MergeObservation(ILookupTemplate<IFhirTemplate> lookup, IObservationGroup observationGroup, TObservation existingObservation)
        {
            EnsureArg.IsNotNull(observationGroup, nameof(observationGroup));

            var (template, processor) = GetTemplateAndProcessor(observationGroup.Name, lookup);
            return processor.MergeObservation(template, observationGroup, existingObservation);
        }

        private (IFhirTemplate template, IFhirTemplateProcessor<IFhirTemplate, TObservation> processor) GetTemplateAndProcessor(string lookupValue, ILookupTemplate<IFhirTemplate> lookup)
        {
            EnsureArg.IsNotNullOrWhiteSpace(lookupValue, nameof(lookupValue));
            EnsureArg.IsNotNull(lookup, nameof(lookup));

            var template = lookup.GetTemplate(lookupValue);
            var templateType = template.GetType();

            if (!_registeredTemplateProcessors.TryGetValue(templateType, out var processor))
            {
                throw new NotSupportedException($"No processor registered for template type {templateType}.");
            }

            return (template, processor);
        }
    }
}
