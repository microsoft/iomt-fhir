// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Telemetry
{
    public static class DimensionNames
    {
        /// <summary>
        /// An metric dimension for category
        /// </summary>
        public static string Category => nameof(DimensionNames.Category);

        /// <summary>
        /// A metric dimension for a specific metric name.
        /// </summary>
        public static string Name => nameof(DimensionNames.Name);

        /// <summary>
        /// A metric dimension that represents each ingestion stage of the IoMT Connector.
        /// </summary>
        public static string Operation => nameof(DimensionNames.Operation);

        /// <summary>
        /// A metric dimension for a error type.
        /// </summary>
        public static string ErrorType => nameof(DimensionNames.ErrorType);

        /// <summary>
        /// A metric dimension for error severity.
        /// </summary>
        public static string ErrorSeverity => nameof(DimensionNames.ErrorSeverity);
    }
}
