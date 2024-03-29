﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateOptions
    {
        public const string Settings = "Template";

        public string DeviceContent { get; set; }

        public string FhirMapping { get; set; }
    }
}
