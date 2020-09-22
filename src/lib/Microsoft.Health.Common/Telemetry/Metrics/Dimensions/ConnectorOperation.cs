// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Telemetry
{
    public static class ConnectorOperation
    {
        /// <summary>
        /// The setup stage of the IoMT Connector
        /// </summary>
        public static string Setup => nameof(ConnectorOperation.Setup);

        /// <summary>
        /// The normalization stage of the IoMT Connector
        /// </summary>
        public static string Normalization => nameof(ConnectorOperation.Normalization);

        /// <summary>
        /// The grouping stage of the IoMT Connector
        /// </summary>
        public static string Grouping => nameof(ConnectorOperation.Grouping);

        /// <summary>
        /// The FHIR conversion stage of the IoMT Connector
        /// </summary>
        public static string FHIRConversion => nameof(ConnectorOperation.FHIRConversion);

        /// <summary>
        /// If a stage is not determined.
        /// </summary>
        public static string Unknown => nameof(ConnectorOperation.Unknown);
    }
}
