// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Validation.Models
{
    public enum ValidationCategory
    {
        /// <summary>
        /// Indicates that the area of validation is related to Normalization.
        /// </summary>
        NORMALIZATION,

        /// <summary>
        /// Indicates that the area of validation is related to Fhir Transformation.
        /// </summary>
        FHIRTRANSFORMATION,
    }
}