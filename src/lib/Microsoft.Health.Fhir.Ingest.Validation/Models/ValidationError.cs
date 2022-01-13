// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Validation.Models
{
    public class ValidationError
    {
        public ValidationError(string message, ErrorLevel errorLevel = ErrorLevel.ERROR)
        {
            Message = EnsureArg.IsNotNullOrWhiteSpace(message, nameof(message));
            Level = errorLevel;
        }

        public string Message { get; }

        public ErrorLevel Level { get; }
    }
}
