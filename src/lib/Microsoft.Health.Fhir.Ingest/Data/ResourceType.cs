// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public enum ResourceType
    {
        /// <summary>
        /// Device Resource
        /// </summary>
        Device,

        /// <summary>
        /// Patient Resource
        /// </summary>
        Patient,

        /// <summary>
        /// Encounter Resource
        /// </summary>
        Encounter,

        /// <summary>
        /// Observation Resource
        /// </summary>
        Observation,
    }
}
