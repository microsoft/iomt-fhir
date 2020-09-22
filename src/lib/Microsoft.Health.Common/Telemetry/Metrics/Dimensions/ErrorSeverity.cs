// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Telemetry
{
    public static class ErrorSeverity
    {
        /// <summary>
        /// An error severity level of Critical
        /// </summary>
        public static string Critical => nameof(ErrorSeverity.Critical);

        /// <summary>
        /// An error severity level of Warning
        /// </summary>
        public static string Warning => nameof(ErrorSeverity.Warning);

        /// <summary>
        /// An error serverity level of Informational
        /// </summary>
        public static string Informational => nameof(ErrorSeverity.Informational);
    }
}
