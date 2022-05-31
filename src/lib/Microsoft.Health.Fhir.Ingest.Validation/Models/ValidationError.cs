// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Validation.Models
{
    public class ValidationError
    {
        public ValidationError(string message, ValidationCategory category, ILineInfo lineInfo = null, ErrorLevel errorLevel = ErrorLevel.ERROR)
        {
            Message = EnsureArg.IsNotNullOrWhiteSpace(message, nameof(message));
            Level = errorLevel;
            Category = category;
            LineInfo = lineInfo;
        }

        public string Message { get; }

        public ErrorLevel Level { get; }

        public ValidationCategory Category { get; }

        public ILineInfo LineInfo { get; }
    }
}
