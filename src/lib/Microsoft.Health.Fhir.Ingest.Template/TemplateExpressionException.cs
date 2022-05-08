// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateExpressionException : InvalidTemplateException, IExceptionWithLineInfo
    {
        private readonly LineInfo _lineInfo;

        public TemplateExpressionException(string message, LineInfo lineInfo)
             : this(message, null, lineInfo)
        {
        }

        public TemplateExpressionException(string message, Exception innerException, LineInfo lineInfo)
            : base(message, innerException)
        {
            EnsureArg.IsNotNull(lineInfo, nameof(lineInfo));
            _lineInfo = lineInfo;
        }

        public TemplateExpressionException()
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

        public LineInfo GetLineInfo => _lineInfo;

        public bool HasLineInfo => _lineInfo.HasLineInfo();
    }
}
