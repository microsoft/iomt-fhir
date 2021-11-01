// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Events.Telemetry.Exceptions
{
    public class InvalidEventHubException :
        Exception,
        ITelemetryFormattable
    {
        private readonly string _metricName;

        public InvalidEventHubException()
        {
        }

        public InvalidEventHubException(string message)
            : base(message)
        {
        }

        public InvalidEventHubException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidEventHubException(string message, Exception innerException, string helpLink, string errorName)
            : base(message, innerException)
        {
            EnsureArg.IsNotNullOrWhiteSpace(errorName, nameof(errorName));

            HelpLink = helpLink;
            _metricName = $"{ErrorType.EventHubError}{errorName}";
        }

        public Metric ToMetric => new Metric(
            _metricName,
            new Dictionary<string, object>
            {
                { DimensionNames.Name, _metricName },
                { DimensionNames.Category, Category.Errors },
                { DimensionNames.ErrorType, ErrorType.EventHubError },
                { DimensionNames.ErrorSeverity, ErrorSeverity.Warning },
                { DimensionNames.ErrorSource, ErrorSource.User },
                { DimensionNames.Operation, ConnectorOperation.Setup },
            });
    }
}
