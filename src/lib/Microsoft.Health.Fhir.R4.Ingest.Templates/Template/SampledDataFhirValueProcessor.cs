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

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class SampledDataFhirValueProcessor : FhirValueProcessor<SampledDataFhirValueType, IObservationData, DataType>
    {
        private readonly SampledDataProcessor _sampledDataProcessor;

        public SampledDataFhirValueProcessor(SampledDataProcessor sampledDataProcessor = null)
        {
            _sampledDataProcessor = sampledDataProcessor ?? SampledDataProcessor.Instance;
        }

        protected override DataType CreateValueImpl(SampledDataFhirValueType template, IObservationData inValue)
        {
            EnsureArg.IsNotNull(template, nameof(template));
            EnsureArg.IsNotNull(inValue, nameof(inValue));
            IEnumerable<(DateTime, string)> values = EnsureArg.IsNotNull(inValue.Data, nameof(IObservationData.Data));

            (DateTime observationStart, DateTime observationEnd) = inValue.ObservationPeriod;

            return new SampledData
            {
                Origin = new Quantity { Value = 0, Unit = template.Unit },
                Period = template.DefaultPeriod,
                Dimensions = 1,
                Data = _sampledDataProcessor.BuildSampledData(values.ToArray(), observationStart, observationEnd, template.DefaultPeriod),
            };
        }

        protected override DataType MergeValueImpl(SampledDataFhirValueType template, IObservationData inValue, DataType existingValue)
        {
            EnsureArg.IsNotNull(template, nameof(template));
            EnsureArg.IsNotNull(inValue, nameof(inValue));
            IEnumerable<(DateTime, string)> values = EnsureArg.IsNotNull(inValue.Data, nameof(IObservationData.Data));

            if (!(existingValue is SampledData sampledData))
            {
                throw new NotSupportedException($"Element {nameof(existingValue)} expected to be of type {typeof(SampledData)}.");
            }

            if (sampledData.Dimensions > 1)
            {
                throw new NotSupportedException($"Existing {typeof(SampledData)} value has more than 1 dimension.");
            }

            (DateTime dataStart, DateTime dataEnd) = inValue.DataPeriod;
            (DateTime observationStart, DateTime observationEnd) = inValue.ObservationPeriod;

            DateTime mergeStart = dataStart < observationStart ? dataStart : observationStart;
            DateTime mergeEnd = dataEnd > observationEnd ? dataEnd : observationEnd;

            var existingTimeValues = _sampledDataProcessor.SampledDataToTimeValues(sampledData.Data, observationStart, template.DefaultPeriod);
            var mergedTimeValues = _sampledDataProcessor.MergeData(existingTimeValues, values.ToArray());
            sampledData.Data = _sampledDataProcessor.BuildSampledData(mergedTimeValues, mergeStart, mergeEnd, template.DefaultPeriod);

            return existingValue;
        }
    }
}
