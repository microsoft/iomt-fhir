// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Validation.Models
{
    public enum ErrorLevel
    {
        /// <summary>
        /// Indicates an error that will prevent the mapping process from succeeding.
        /// </summary>
        ERROR,

        /// <summary>
        /// Indicates an issue with a mapping operation that may result in an unexpected outcome for the user
        /// </summary>
        WARN,
    }
}
