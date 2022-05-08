// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class IncompatibleDataException : IomtTelemetryFormattableException, IExceptionWithLineInfo
    {
        private readonly LineInfo _lineInfo;

        public IncompatibleDataException()
            : this(null, new LineInfo())
        {
        }

        public IncompatibleDataException(string message, LineInfo lineInfo)
            : this(message, null, lineInfo)
        {
        }

        public IncompatibleDataException(string message, Exception innerException, LineInfo lineInfo)
            : base(message, innerException)
        {
            _lineInfo = EnsureArg.IsNotNull(lineInfo, nameof(lineInfo));
        }

        public override string ErrName => nameof(IncompatibleDataException);

        public override string ErrType => ErrorType.DeviceMessageError;

        public override string ErrSource => nameof(ErrorSource.User);

        public override string Operation => ConnectorOperation.Normalization;

        public override string Message
        {
            get
            {
                return _lineInfo.HasLineInfo() ? $"Line Number: {_lineInfo.LineNumber}, Position: {_lineInfo.LinePosition}. {base.Message}" : base.Message;
            }
        }

        public LineInfo GetLineInfo => _lineInfo;

        public bool HasLineInfo => _lineInfo.HasLineInfo();
    }
}
