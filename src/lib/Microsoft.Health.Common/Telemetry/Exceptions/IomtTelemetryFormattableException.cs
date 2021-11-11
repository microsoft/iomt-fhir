// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Common.Telemetry.Exceptions
{
    public class IomtTelemetryFormattableException :
        Exception,
        ITelemetryFormattable
    {
        private readonly string _name;
        private readonly string _operation = ConnectorOperation.Unknown;

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

        public virtual string ErrName => _name;

        public virtual string Operation => _operation;

        public Metric ToMetric => ErrName.ToErrorMetric(Operation, ErrType, ErrSeverity, ErrSource);
    }
}
