// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.Health.Logging.Telemetry.Exceptions
{
    public static class ExceptionExtensionMethods
    {
        public static void LogException(this Exception ex, TelemetryClient telemetryClient)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(telemetryClient, nameof(telemetryClient));

            var exceptionTelemetry = new ExceptionTelemetry(ex);

            exceptionTelemetry.Properties.Add("message", ex.Message ?? string.Empty);

            telemetryClient.TrackException(exceptionTelemetry);
        }
    }
}
