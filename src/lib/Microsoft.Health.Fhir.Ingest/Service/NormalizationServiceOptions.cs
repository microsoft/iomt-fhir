// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class NormalizationServiceOptions
    {
        public const string Settings = "NormalizationService";

        public bool LogDeviceIngressSizeBytes { get; set; } = false;
    }
}
