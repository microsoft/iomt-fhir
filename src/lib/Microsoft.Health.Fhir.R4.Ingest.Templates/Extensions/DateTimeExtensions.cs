// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using EnsureThat;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.R4.Ingest.Templates.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToUtcDateTime(this FhirDateTime fhirDateTime)
        {
            return EnsureArg.IsNotNull(fhirDateTime, nameof(fhirDateTime))
                .ToDateTimeOffset(TimeSpan.Zero)
                .UtcDateTime;
        }

        public static (DateTime start, DateTime end) ToUtcDateTimePeriod(this Period period)
        {
            EnsureArg.IsNotNull(period, nameof(period));

            return (period.StartElement.ToUtcDateTime(), period.EndElement.ToUtcDateTime());
        }

        public static Period ToPeriod(this (DateTime start, DateTime end) boundary)
        {
            return new Period
            {
                Start = boundary.start.ToString("o", CultureInfo.InvariantCulture.DateTimeFormat),
                End = boundary.end.ToString("o", CultureInfo.InvariantCulture.DateTimeFormat),
            };
        }
    }
}
