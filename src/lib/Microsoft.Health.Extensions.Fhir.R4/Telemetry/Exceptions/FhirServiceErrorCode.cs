// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions
{
    public enum FhirServiceErrorCode
    {
        /// <summary>
        /// Error code that categorizes invalid configurations (e.g. invalid FHIR service URL)
        /// </summary>
        ConfigurationError,

        /// <summary>
        /// Error code that categorizes authentication errors (e.g. HTTP status 401/Unauthorized)
        /// </summary>
        AuthorizationError,

        /// <summary>
        /// Error code that categorizes all other generic exceptions
        /// </summary>
        GeneralError,
    }
}
