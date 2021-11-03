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
        public static string Category => nameof(Category);

        /// <summary>
        /// A metric dimension for a specific metric name.
        /// </summary>
        public static string Name => nameof(Name);

        /// <summary>
        /// A metric dimension that represents each ingestion stage of the IoMT Connector.
        /// </summary>
        public static string Operation => nameof(Operation);

        /// <summary>
        /// A metric dimension that represents an identifier related to the metric emitted.
        /// </summary>
        public static string Identifier => nameof(Identifier);

        /// <summary>
        /// A metric dimension for a error type.
        /// </summary>
        public static string ErrorType => nameof(ErrorType);

        /// <summary>
        /// A metric dimension for error severity.
        /// </summary>
        public static string ErrorSeverity => nameof(ErrorSeverity);

        /// <summary>
        /// A metric dimension to identify the error source, e.g. the user or service.
        /// </summary>
        public static string ErrorSource => nameof(ErrorSource);

        /// <summary>
        /// A metric dimension that represents the reason that caused the metric to be emitted.
        /// </summary>
        public static string Reason => nameof(Reason);
    }
}
