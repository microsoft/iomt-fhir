// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateExpressionException : InvalidTemplateException
    {
        public TemplateExpressionException(string message, ILineInfo lineInfo)
             : base(message, lineInfo)
        {
        }

        public TemplateExpressionException(string message, Exception innerException, ILineInfo lineInfo)
            : base(message, innerException, lineInfo)
        {
        }

        public TemplateExpressionException()
        {
        }
    }
}
