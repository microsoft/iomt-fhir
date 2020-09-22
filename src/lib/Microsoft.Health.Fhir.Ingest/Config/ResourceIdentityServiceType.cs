// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Config
{
    /// <summary>
    /// Resource Identity Service Type.
    /// </summary>
    public enum ResourceIdentityServiceType
    {
        /// <summary>
        /// Create the identity resources if they do not exist.
        /// </summary>
        Create,

        /// <summary>
        /// Look up the identity resources.
        /// </summary>
        Lookup,

        /// <summary>
        /// Look up the identity resource with the Encounter info.
        /// </summary>
        LookupWithEncounter,
    }
}
