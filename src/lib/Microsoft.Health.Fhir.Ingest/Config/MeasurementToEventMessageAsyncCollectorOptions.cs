// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Config
{
    public class MeasurementToEventMessageAsyncCollectorOptions
    {
        public const string Settings = "CollectorOptions";

        public bool UseCompressionOnSend { get; set; } = false;
    }
}
