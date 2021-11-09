// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Common.Telemetry
{
    public class IomtTelemetryFormattableException :
        Exception,
        ITelemetryFormattable
    {
        private readonly string _name;
        private readonly string _operation;

        public IomtTelemetryFormattableException()
        {
        }

        public IomtTelemetryFormattableException(string message)
            : base(message)
        {
        }

        public IomtTelemetryFormattableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public IomtTelemetryFormattableException(
            string message,
            Exception innerException,
            string name,
            string operation)
            : base(message, innerException)
        {
            _name = EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
            _operation = EnsureArg.IsNotNullOrWhiteSpace(operation, nameof(operation));
        }

        public virtual string ErrType => ErrorType.GeneralError;

        public virtual string ErrSeverity => ErrorSeverity.Warning;

        public virtual string ErrSource => nameof(ErrorSource.Undefined);

        public Metric ToMetric => _name.ToErrorMetric(_operation, ErrType, ErrSeverity, ErrSource);
    }
}
