// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public enum ResourceOperation
    {
        /// <summary>
        /// FHIR resource created
        /// </summary>
        Created,

        /// <summary>
        /// FHIR resource updated
        /// </summary>
        Updated,

        /// <summary>
        /// FHIR resource no operation performed
        /// </summary>
        NoOperation,
    }
}
