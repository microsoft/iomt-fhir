// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnsureThat;

namespace Microsoft.Health.Common.Telemetry
{
    public class IomtException :
        Exception,
        ITelemetryFormattable
    {
        private readonly string _name;
        private readonly string _errorType;
        private readonly string _errorSeverity;
        private readonly string _errorSource;
        private readonly string _operation;

        public IomtException()
        {
        }

        public IomtException(string message)
            : base(message)
        {
        }

        public IomtException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public IomtException(
            string message,
            Exception innerException,
            string helpLink,
            string name,
            string errorType,
            string errorSeverity,
            string errorSource,
            string operation)
            : base(message, innerException)
        {
            _name = EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));

            HelpLink = helpLink;
            _errorType = errorType;
            _errorSeverity = errorSeverity;
            _errorSource = errorSource;
            _operation = operation;
        }

        public Metric ToMetric => new Metric(
            EnsureArg.IsNotNullOrWhiteSpace(_name, nameof(_name)),
            new Dictionary<string, object>
            {
                { DimensionNames.Category, Category.Errors },
            })
            .AddDimension(DimensionNames.Name, _name)
            .AddDimension(DimensionNames.ErrorType, _errorType)
            .AddDimension(DimensionNames.ErrorSeverity, _errorSeverity)
            .AddDimension(DimensionNames.ErrorSource, _errorSource)
            .AddDimension(DimensionNames.Operation, _operation);
    }
}
