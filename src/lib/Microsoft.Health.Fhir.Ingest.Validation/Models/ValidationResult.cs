// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Validation.Models
{
    public class ValidationResult
    {
        public TemplateResult TemplateResult { get; set; } = new TemplateResult();

        public IList<DeviceResult> DeviceResults { get; set; } = new List<DeviceResult>();
    }
}