// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public enum NormalizationErrorHandlingPolicy
    {
        /// <summary>
        /// Retry mapping errors infinitely.
        /// </summary>
        Retry = 0,

        /// <summary>
        /// Ignore errors during mapping match and extract phase.
        /// </summary>
        DiscardMatchAndExtractErrors = 1,
    }
}
