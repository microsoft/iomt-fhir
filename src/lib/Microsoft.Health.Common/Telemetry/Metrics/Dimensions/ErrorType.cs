// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Telemetry
{
    public static class ErrorType
    {
        /// <summary>
        /// A metric type for authentication errors
        /// </summary>
        public static string AuthenticationError => nameof(AuthenticationError);

        /// <summary>
        /// A metric type for device template errors
        /// </summary>
        public static string DeviceTemplateError => nameof(DeviceTemplateError);

        /// <summary>
        /// A metric type for device message errors
        /// </summary>
        public static string DeviceMessageError => nameof(DeviceMessageError);

        /// <summary>
        /// A metric type for errors that occur when interacting with event hub resources.
        /// </summary>
        public static string EventHubError => nameof(EventHubError);

        /// <summary>
        /// A metric type for errors that occur when loading or parsing the FHIR template json file.
        /// </summary>
        public static string FHIRTemplateError => nameof(FHIRTemplateError);

        /// <summary>
        /// A metric type for errors that occur when converting the normalized payload to FHIR.
        /// </summary>
        public static string FHIRConversionError => nameof(FHIRConversionError);

        /// <summary>
        /// A metric type for errors that occur when interacting with FHIR resources.
        /// </summary>
        public static string FHIRResourceError => nameof(FHIRResourceError);

        /// <summary>
        /// A metric type for errors that occur when interacting with the FHIR server.
        /// </summary>
        public static string FHIRServiceError => nameof(FHIRServiceError);

        /// <summary>
        /// A metric type for errors of unknown type (e.g. unhandled exceptions)
        /// </summary>
        public static string GeneralError => nameof(GeneralError);

        /// <summary>
        /// A metric type for information
        /// </summary>
        public static string ServiceInformation => nameof(ServiceInformation);
    }
}
