// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Tools.DataMapper.Models
{
    /// <summary>
    /// Transformation testing request.
    /// </summary>
    public class TransformationTestRequest
    {
        /// <summary>
        /// Gets or sets FHIR Mapping in string.
        /// </summary>
        public string FhirMapping { get; set; }

        /// <summary>
        /// Gets or sets data to normalize.
        /// </summary>
        public string NormalizedData { get; set; }

        /// <summary>
        /// Gets or sets FHIR Version to use.
        /// </summary>
        public string FhirVersion { get; set; }

        /// <summary>
        /// Gets or sets Identity Resolution Type.
        /// </summary>
        public string FhirIdentityResolutionType { get; set; }
    }
}
