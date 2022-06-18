// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateError
    {
        public TemplateError(string message)
        {
            Message = message;
            LineInfo = new LineInfo();
        }

        public TemplateError(string message, ILineInfo lineInfo)
        {
            Message = message;
            LineInfo = EnsureArg.IsNotNull(lineInfo, nameof(lineInfo));
        }

        public string Message { get; }

        public ILineInfo LineInfo { get; }
    }
}
