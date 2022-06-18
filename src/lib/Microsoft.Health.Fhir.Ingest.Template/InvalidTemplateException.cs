// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class InvalidTemplateException : Exception, IExceptionWithLineInfo
    {
        private readonly ILineInfo _lineInfo;

        public InvalidTemplateException(string message, ILineInfo lineInfo)
            : this(message, null, lineInfo)
        {
        }

        public InvalidTemplateException(string message, Exception innerException, ILineInfo lineInfo)
            : base(message, innerException)
        {
            _lineInfo = EnsureArg.IsNotNull(lineInfo, nameof(lineInfo));
        }

        public InvalidTemplateException()
            : this(null, new LineInfo())
        {
        }

        public override string Message
        {
            get
            {
                return _lineInfo.HasLineInfo() ? $"Line Number: {_lineInfo.LineNumber}, Position: {_lineInfo.LinePosition}. {base.Message}" : base.Message;
            }
        }

        public ILineInfo GetLineInfo => _lineInfo;

        public bool HasLineInfo => _lineInfo.HasLineInfo();
    }
}
