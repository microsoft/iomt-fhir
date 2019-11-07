// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;
using Microsoft.Extensions.Logging;

namespace Microsoft.Health.Common.Telemetry
{
    public static class TelemetryExtensions
    {
        public static ILogger RecordUnhandledExceptionMetrics(this ILogger logger, Exception ex, string context)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNullOrWhiteSpace(context, nameof(context));
            logger.LogMetric("UnhandledException", 1, new Dictionary<string, object> { { "ExceptionType", ex.GetType() }, { "Context", context } });
            return logger;
        }
    }
}
