// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Hl7.Fhir.Rest;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Identity.Client;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Extensions.Fhir.R4.UnitTests
{
    public class FhirServiceExceptionProcessorTests
    {
        private static readonly Exception _fhirForbiddenEx = new FhirOperationException("test", HttpStatusCode.Forbidden);
        private static readonly Exception _fhirNotFoundEx = new FhirOperationException("test", HttpStatusCode.NotFound);
        private static readonly Exception _fhirBadRequestEx = new FhirOperationException("test", HttpStatusCode.BadRequest);
        private static readonly Exception _argEndpointNullEx = new ArgumentNullException("endpoint");
        private static readonly Exception _argEndpointEx = new ArgumentException("endpoint", "Endpoint must be absolute");
        private static readonly Exception _argEx = new ArgumentException("test_message", "test_param");
        private static readonly Exception _uriEx = new UriFormatException();
        private static readonly Exception _httpNotKnownEx = new HttpRequestException("Name or service not known");
        private static readonly Exception _httpEx = new HttpRequestException();
        private static readonly Exception _msalInvalidResourceEx = new MsalServiceException("invalid_resource", "test_message");
        private static readonly Exception _msalInvalidScopeEx = new MsalServiceException("invalid_scope", "test_message");
        private static readonly Exception _msalEx = new MsalServiceException("test_code", "test_message");
        private static readonly Exception _ex = new Exception();

        public static IEnumerable<object[]> ProcessExceptionData =>
            new List<object[]>
            {
                new object[] { _fhirForbiddenEx, "FHIRServiceErrorAuthorizationError", nameof(ErrorSource.User) },
                new object[] { _fhirNotFoundEx, "FHIRServiceErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _fhirBadRequestEx, "FHIRServiceErrorBadRequest" },
                new object[] { _argEndpointNullEx, "FHIRServiceErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _argEndpointEx, "FHIRServiceErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _argEx, "FHIRServiceErrorArgumentErrortest_param" },
                new object[] { _uriEx, "FHIRServiceErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _httpNotKnownEx, "FHIRServiceErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _httpEx, "FHIRServiceErrorHttpRequestError" },
                new object[] { _msalInvalidResourceEx, "FHIRServiceErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _msalInvalidScopeEx, "FHIRServiceErrorConfigurationError", nameof(ErrorSource.User) },
                new object[] { _msalEx, "FHIRServiceErrorMsalServiceErrortest_code" },
                new object[] { _ex, "FHIRServiceErrorGeneralError" },
            };

        public static IEnumerable<object[]> CustomizeExceptionData =>
            new List<object[]>
            {
                new object[] { _fhirForbiddenEx, typeof(UnauthorizedAccessFhirServiceException) },
                new object[] { _fhirNotFoundEx, typeof(InvalidFhirServiceException) },
                new object[] { _fhirBadRequestEx, typeof(FhirOperationException) },
                new object[] { _argEndpointNullEx, typeof(InvalidFhirServiceException) },
                new object[] { _argEndpointEx, typeof(InvalidFhirServiceException) },
                new object[] { _argEx, typeof(ArgumentException) },
                new object[] { _uriEx, typeof(InvalidFhirServiceException) },
                new object[] { _httpNotKnownEx, typeof(InvalidFhirServiceException) },
                new object[] { _httpEx, typeof(HttpRequestException) },
                new object[] { _msalInvalidResourceEx, typeof(InvalidFhirServiceException) },
                new object[] { _msalInvalidScopeEx, typeof(InvalidFhirServiceException) },
                new object[] { _msalEx, typeof(MsalServiceException) },
                new object[] { _ex, typeof(Exception) },
            };

        [Theory]
        [MemberData(nameof(ProcessExceptionData))]
        public void GivenExceptionType_WhenProcessException_ThenExceptionLoggedAndErrorMetricLogged_Test(Exception ex, string expectedErrorMetricName, string expectedErrorSource = null)
        {
            var logger = Substitute.For<ITelemetryLogger>();

            FhirServiceExceptionProcessor.ProcessException(ex, logger);

            logger.ReceivedWithAnyArgs(1).LogError(ex);
            logger.Received(1).LogMetric(
                Arg.Is<Metric>(m =>
                    m.Name.Equals(expectedErrorMetricName) &&
                    m.Dimensions[DimensionNames.Name].Equals(expectedErrorMetricName) &&
                    m.Dimensions[DimensionNames.Operation].Equals(ConnectorOperation.FHIRConversion) &&
                    m.Dimensions[DimensionNames.Category].Equals(Category.Errors) &&
                    m.Dimensions[DimensionNames.ErrorType].Equals(ErrorType.FHIRServiceError) &&
                    m.Dimensions[DimensionNames.ErrorSeverity].Equals(ErrorSeverity.Critical) &&
                    (string.IsNullOrWhiteSpace(expectedErrorSource) ? !m.Dimensions.ContainsKey(DimensionNames.ErrorSource) : m.Dimensions[DimensionNames.ErrorSource].Equals(expectedErrorSource))),
                1);
        }

        [Theory]
        [MemberData(nameof(CustomizeExceptionData))]
        public void GivenExceptionType_WhenCustomizeException_ThenCustomExceptionTypeReturned_Test(Exception ex, Type customExType)
        {
            var (customEx, errName) = FhirServiceExceptionProcessor.CustomizeException(ex);

            Assert.IsType(customExType, customEx);
        }
    }
}
