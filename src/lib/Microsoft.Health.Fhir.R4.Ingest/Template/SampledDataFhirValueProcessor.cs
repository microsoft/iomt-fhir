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
    public class SampledDataFhirValueProcessor : FhirValueProcessor<SampledDataFhirValueType, IObservationData, Element>
    {
        private readonly SampledDataProcessor _sampledDataProcessor;

        public SampledDataFhirValueProcessor(SampledDataProcessor sampledDataProcessor = null)
        {
            _sampledDataProcessor = sampledDataProcessor ?? SampledDataProcessor.Instance;
        }

        protected override Element CreateValueImpl(SampledDataFhirValueType template, IObservationData inValue)
        {
            EnsureArg.IsNotNull(template, nameof(template));
            EnsureArg.IsNotNull(inValue, nameof(inValue));
            IEnumerable<(DateTime, string)> values = EnsureArg.IsNotNull(inValue.Data, nameof(IObservationData.Data));
            DateTime dataStart = inValue.DataPeriod.start;
            DateTime dataEnd = inValue.DataPeriod.end;

            return new SampledData
            {
                Origin = new SimpleQuantity { Value = 0, Unit = template.Unit },
                Period = template.DefaultPeriod,
                Dimensions = 1,
                Data = _sampledDataProcessor.BuildSampledData(values.ToArray(), dataStart, dataEnd, template.DefaultPeriod),
            };
        }

        protected override Element MergeValueImpl(SampledDataFhirValueType template, IObservationData inValue, Element existingValue)
        {
            EnsureArg.IsNotNull(template, nameof(template));
            EnsureArg.IsNotNull(inValue, nameof(inValue));
            IEnumerable<(DateTime, string)> values = EnsureArg.IsNotNull(inValue.Data, nameof(IObservationData.Data));
            DateTime dataStart = inValue.DataPeriod.start;
            DateTime dataEnd = inValue.DataPeriod.end;

            if (!(existingValue is SampledData sampledData))
            {
                throw new NotSupportedException($"Element {nameof(existingValue)} expected to be of type {typeof(SampledData)}.");
            }

            if (sampledData.Dimensions > 1)
            {
                throw new NotSupportedException($"Existing {typeof(SampledData)} value has more than 1 dimension.");
            }

            var existingTimeValues = _sampledDataProcessor.SampledDataToTimeValues(sampledData.Data, dataStart, template.DefaultPeriod);
            var mergedTimeValues = _sampledDataProcessor.MergeData(existingTimeValues, values.ToArray());
            sampledData.Data = _sampledDataProcessor.BuildSampledData(mergedTimeValues, dataStart, dataEnd, template.DefaultPeriod);

            return existingValue;
        }
    }
}
