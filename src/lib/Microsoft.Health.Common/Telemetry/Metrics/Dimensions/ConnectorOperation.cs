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
        public static string Setup => nameof(Setup);

        /// <summary>
        /// The normalization stage of the IoMT Connector
        /// </summary>
        public static string Normalization => nameof(Normalization);

        /// <summary>
        /// The grouping stage of the IoMT Connector
        /// </summary>
        public static string Grouping => nameof(Grouping);

        /// <summary>
        /// The FHIR conversion stage of the IoMT Connector
        /// </summary>
        public static string FHIRConversion => nameof(FHIRConversion);

        /// <summary>
        /// The measurement to event hub stage of the IoMT Connector.
        /// </summary>
        public static string MeasurementToEventHub => nameof(MeasurementToEventHub);

        /// <summary>
        /// If a stage is not determined.
        /// </summary>
        public static string Unknown => nameof(Unknown);
    }
}
