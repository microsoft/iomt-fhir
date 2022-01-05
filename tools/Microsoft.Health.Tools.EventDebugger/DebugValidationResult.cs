// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Ingest.Validation.Models;

namespace Microsoft.Health.Tools.EventDebugger
{
    public class DebugValidationResult
    {
        public ValidationResult ValidationResult { get; set; } = new ValidationResult();

        public long SequenceNumber { get; set; }
    }
}
