// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public abstract class FhirTemplateProcessor<TTemplate, TObservation> : IFhirTemplateProcessor<IFhirTemplate, TObservation>
        where TTemplate : class, IFhirTemplate
    {
        private static readonly Type SupportedType = typeof(TTemplate);

        public Type SupportedTemplateType => SupportedType;

        public TObservation CreateObservation(IFhirTemplate template, IObservationGroup observationGroup)
        {
            return CreateObservationImpl(CastTemplate(template), observationGroup);
        }

        public IEnumerable<IObservationGroup> CreateObservationGroups(IFhirTemplate template, IMeasurementGroup measurementGroup)
        {
            return CreateObservationGroupsImpl(CastTemplate(template), measurementGroup);
        }

        public TObservation MergeObservation(IFhirTemplate template, IObservationGroup observationGroup, TObservation existingObservation)
        {
            return MergeObservationImpl(CastTemplate(template), observationGroup, existingObservation);
        }

        protected abstract TObservation CreateObservationImpl(TTemplate template, IObservationGroup observationGroup);

        protected abstract TObservation MergeObservationImpl(TTemplate template, IObservationGroup observationGroup, TObservation existingObservation);

        protected abstract IEnumerable<IObservationGroup> CreateObservationGroupsImpl(TTemplate template, IMeasurementGroup measurementGroup);

        private TTemplate CastTemplate(IFhirTemplate template)
        {
            EnsureArg.IsNotNull(template, nameof(template));

            if (!(template is TTemplate castTemplate))
            {
                throw new NotSupportedException($"Template type {template.GetType()} does not match supported type {SupportedTemplateType}.");
            }

            return castTemplate;
        }
    }
}
