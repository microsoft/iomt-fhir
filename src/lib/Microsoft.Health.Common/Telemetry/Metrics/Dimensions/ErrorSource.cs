// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Telemetry
{
    public static class ErrorSource
    {
        /// <summary>
        /// A system error.
        /// </summary>
        public static string System => nameof(System);

        /// <summary>
        /// A user error.
        /// </summary>
        public static string User => nameof(User);
    }
}
