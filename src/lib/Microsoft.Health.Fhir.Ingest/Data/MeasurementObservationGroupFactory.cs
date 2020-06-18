// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class MeasurementObservationGroupFactory : IObservationGroupFactory<IMeasurementGroup>
    {
        private readonly IObservationGroupFactory<IMeasurementGroup> _internalFactory;

        public MeasurementObservationGroupFactory(ObservationPeriodInterval period)
        {
            switch (period)
            {
                case ObservationPeriodInterval.CorrelationId:
                    _internalFactory = new CorrelationMeasurementObservationGroupFactory();
                    break;
                case ObservationPeriodInterval.Single:
                case ObservationPeriodInterval.Hourly:
                case ObservationPeriodInterval.Daily:
                    _internalFactory = new TimePeriodMeasurementObservationGroupFactory(period);
                    break;
                default:
                    throw new NotSupportedException($"ObservationPeriodInterval {period} is not supported.");
            }
        }

        public IEnumerable<IObservationGroup> Build(IMeasurementGroup input)
        {
            return _internalFactory.Build(input);
        }
    }
}
