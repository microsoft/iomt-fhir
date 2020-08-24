// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Telemetry
{
    public static class Category
    {
        /// <summary>
        /// An metric category for errors
        /// </summary>
        public static string Errors => nameof(Category.Errors);

        /// <summary>
        /// A metric category for traffic and requests
        /// </summary>
        public static string Traffic => nameof(Category.Traffic);

        /// <summary>
        /// A metric category for service availability
        /// </summary>
        public static string Availability => nameof(Category.Availability);

        /// <summary>
        /// A metric category for request latency
        /// </summary>
        public static string Latency => nameof(Category.Latency);

        /// <summary>
        /// A metric category for saturation
        /// </summary>
        public static string Saturation => nameof(Category.Saturation);
    }
}
