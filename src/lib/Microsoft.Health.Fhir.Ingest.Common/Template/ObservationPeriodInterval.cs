// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public enum ObservationPeriodInterval
    {
        /// <summary>
        /// Do not group according to time, group according to a correlation id supplied in the measurement group.
        /// </summary>
        CorrelationId = -1,

        /// <summary>
        /// Do not group measurements. Each measurement will be mapped to one observation.
        /// </summary>
#pragma warning disable CA1720
        Single = 0,
#pragma warning restore CA1720

        /// <summary>
        /// Group measurements by one hour intervals.
        /// </summary>
        Hourly = 60,

        /// <summary>
        /// Group measurements by one day intervals.
        /// </summary>
        Daily = 1440,
    }
}
