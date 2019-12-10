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
    public class SampledDataFhirValueProcessor : FhirValueProcessor<SampledDataFhirValueType, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values), Element>
    {
        private readonly SampledDataProcessor _sampledDataProcessor;

        public SampledDataFhirValueProcessor(SampledDataProcessor sampledDataProcessor = null)
        {
            _sampledDataProcessor = sampledDataProcessor ?? SampledDataProcessor.Instance;
        }

        protected override Element CreateValueImpl(SampledDataFhirValueType template, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values) inValue)
        {
            EnsureArg.IsNotNull(template, nameof(template));

            return new SampledData
            {
                Origin = new SimpleQuantity { Value = 0, Unit = template.Unit },
                Period = template.DefaultPeriod,
                Dimensions = 1,
                Data = _sampledDataProcessor.BuildSampledData(inValue.values.ToArray(), inValue.start, inValue.end, template.DefaultPeriod),
            };
        }

        protected override Element MergeValueImpl(SampledDataFhirValueType template, (DateTime start, DateTime end, IEnumerable<(DateTime, string)> values) inValue, Element existingValue)
        {
            EnsureArg.IsNotNull(template, nameof(template));

            if (!(existingValue is SampledData sampledData))
            {
                throw new NotSupportedException($"Element {nameof(existingValue)} expected to be of type {typeof(SampledData)}.");
            }

            if (sampledData.Dimensions > 1)
            {
                throw new NotSupportedException($"Existing {typeof(SampledData)} value has more than 1 dimension.");
            }

            var existingTimeValues = _sampledDataProcessor.SampledDataToTimeValues(sampledData.Data, inValue.start, template.DefaultPeriod);
            var mergedTimeValues = _sampledDataProcessor.MergeData(existingTimeValues, inValue.values.ToArray());
            sampledData.Data = _sampledDataProcessor.BuildSampledData(mergedTimeValues, inValue.start, inValue.end, template.DefaultPeriod);

            return existingValue;
        }
    }
}
