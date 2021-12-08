// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Extensions.Fhir.Telemetry.Metrics;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Identity.Client;

namespace Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions
{
    public static class FhirServiceExceptionProcessor
    {
        private static readonly IExceptionTelemetryProcessor _exceptionTelemetryProcessor = new ExceptionTelemetryProcessor();

        public static void ProcessException(Exception exception, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            var (customException, errorName) = CustomizeException(exception);

            logger.LogError(customException);

            string exceptionName = customException.Equals(exception) ? $"{ErrorType.FHIRServerError}{errorName}" : customException.GetType().Name;
            _exceptionTelemetryProcessor.LogExceptionMetric(customException, logger, FhirClientMetrics.HandledException(exceptionName, ErrorSeverity.Critical));
        }

        public static (Exception customException, string errorName) CustomizeException(Exception exception)
        {
            EnsureArg.IsNotNull(exception, nameof(exception));

            string message;
            string errorName;

            switch (exception)
            {
                case FhirOperationException _:
                    var status = ((FhirOperationException)exception).Status;
                    switch (status)
                    {
                        case HttpStatusCode.Forbidden:
                            message = "Verify that the provided FHIR service's 'FHIR Data Writer' role has been assigned to the applicable Azure Active Directory security principal or managed identity.";
                            string helpLink = "https://docs.microsoft.com/azure/healthcare-apis/iot/deploy-iot-connector-in-azure#accessing-the-iot-connector-from-the-fhir-service";
                            errorName = nameof(FhirServiceErrorCode.AuthorizationError);
                            return (new UnauthorizedAccessFhirServiceException(message, exception, helpLink, errorName), errorName);
                        case HttpStatusCode.NotFound:
                            message = "Verify that the provided URL is for a FHIR service.";
                            errorName = nameof(FhirServiceErrorCode.ConfigurationError);
                            return (new InvalidFhirServiceException(message, exception, errorName), errorName);
                        default:
                            return (exception, status.ToString());
                    }

                case UriFormatException _:
                    message = "Verify that the provided FHIR service URL is an absolute URL.";
                    errorName = nameof(FhirServiceErrorCode.ConfigurationError);
                    return (new InvalidFhirServiceException(message, exception, errorName), errorName);

                case HttpRequestException _:
                    message = "Verify that the provided FHIR service URL exists.";
                    errorName = nameof(FhirServiceErrorCode.ConfigurationError);
                    return (new InvalidFhirServiceException(message, exception, errorName), errorName);

                case MsalServiceException _:
                    string errorCode = ((MsalServiceException)exception).ErrorCode;
                    message = "Verify that the provided FHIR service URL is a base URL and exists in the Microsoft tenant.";
                    errorName = $"{nameof(FhirServiceErrorCode.ConfigurationError)}_{errorCode}";
                    return (new InvalidFhirServiceException(message, exception, errorName), errorName);

                default:
                    return (exception, nameof(FhirServiceErrorCode.GeneralError));
            }
        }
    }
}
