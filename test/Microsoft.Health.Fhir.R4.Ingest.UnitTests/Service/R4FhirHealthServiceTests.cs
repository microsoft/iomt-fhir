// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net;
using Hl7.Fhir.Rest;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4FhirHealthServiceTests
    {
        [Fact]
        public async void GivenValidFhirClientConfig_WhenCheckHealthAsync_ThenRespondWithSuccess_Test()
        {
            var handler = Utilities.CreateMockMessageHandler()
                .Mock(m => m.GetReturnContent(default).ReturnsForAnyArgs(new Model.Bundle()));
            var fhirClient = Utilities.CreateMockFhirClient(handler);

            var service = new R4FhirHealthService(fhirClient);
            var response = await service.CheckHealth();

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(string.Empty, response.Message);
        }

        [Fact]
        public async void GivenInvalidOAuthToken_WhenCheckHealthAsync_ThenRespondWithFhirOperationException_Test()
        {
            var handler = Utilities.CreateMockMessageHandler()
                .Mock(m => m.GetReturnContent(default).ThrowsForAnyArgs(new FhirOperationException("Unauthorized", HttpStatusCode.Unauthorized)));
            var fhirClient = Utilities.CreateMockFhirClient(handler);

            var service = new R4FhirHealthService(fhirClient);
            var response = await service.CheckHealth();

            Assert.Equal(401, response.StatusCode);
            Assert.Equal("Unauthorized", response.Message);
        }

        [Fact]
        public async void GivenInvalidClientSecret_WhenCheckHealthAsync_ThenRespondWithAADException_Test()
        {
            var handler = Utilities.CreateMockMessageHandler()
               .Mock(m => m.GetReturnContent(default).ThrowsForAnyArgs(new IdentityModel.Clients.ActiveDirectory.AdalServiceException("AADSTS123", "Unauthorized") { StatusCode = 401 }));
            var fhirClient = Utilities.CreateMockFhirClient(handler);

            var service = new R4FhirHealthService(fhirClient);
            var response = await service.CheckHealth();

            Assert.Equal(401, response.StatusCode);
            Assert.Equal("Unauthorized", response.Message);
        }

        [Fact]
        public async void GivenInvalidUrl_WhenCheckHealthAsync_ThenRespondWithGenericException_Test()
        {
            var handler = Utilities.CreateMockMessageHandler()
               .Mock(m => m.GetReturnContent(default).ThrowsForAnyArgs(new Exception("No such host is known")));
            var fhirClient = Utilities.CreateMockFhirClient(handler);

            var service = new R4FhirHealthService(fhirClient);
            var response = await service.CheckHealth();

            Assert.Equal(500, response.StatusCode);
            Assert.Equal("No such host is known", response.Message);
        }
    }
}
