// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using Hl7.Fhir.Model;
using Microsoft.Health.Common;
using Microsoft.Health.Fhir.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4FhirHealthServiceTests
    {
        [Fact]
        public async void GivenValidFhirClientConfig_WhenCheckHealthAsync_ThenRespondWithSuccess_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            fhirClient.SearchForResourceAsync(Arg.Any<ResourceType>(), Arg.Any<string>(), Arg.Any<int>(), default).ReturnsForAnyArgs(new FhirResponse<Bundle>(new HttpResponseMessage(HttpStatusCode.OK), null));

            var service = new R4FhirHealthService(fhirClient);
            var response = await service.CheckHealth();

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(string.Empty, response.Message);
        }

        [Fact]
        public async void GivenInvalidOAuthToken_WhenCheckHealthAsync_ThenRespondWithFhirOperationException_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            fhirClient.SearchForResourceAsync(Arg.Any<ResourceType>(), Arg.Any<string>(), Arg.Any<int>(), default).ThrowsForAnyArgs(new FhirException(new FhirResponse<OperationOutcome>(new HttpResponseMessage(HttpStatusCode.Unauthorized), new OperationOutcome())));

            var service = new R4FhirHealthService(fhirClient);
            var response = await service.CheckHealth();

            Assert.Equal(401, response.StatusCode);
            Assert.Equal("Unauthorized: ", response.Message);
        }

        [Fact]
        public async void GivenInvalidClientSecret_WhenCheckHealthAsync_ThenRespondWithAADException_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            fhirClient.SearchForResourceAsync(Arg.Any<ResourceType>(), Arg.Any<string>(), Arg.Any<int>(), default).ThrowsForAnyArgs(new IdentityModel.Clients.ActiveDirectory.AdalServiceException("AADSTS123", "Unauthorized") { StatusCode = 401 });

            var service = new R4FhirHealthService(fhirClient);
            var response = await service.CheckHealth();

            Assert.Equal(401, response.StatusCode);
            Assert.Equal("Unauthorized", response.Message);
        }

        [Fact]
        public async void GivenInvalidUrl_WhenCheckHealthAsync_ThenRespondWithGenericException_Test()
        {
            var fhirClient = Utilities.CreateMockFhirService();
            fhirClient.SearchForResourceAsync(Arg.Any<ResourceType>(), Arg.Any<string>(), Arg.Any<int>(), default).ThrowsForAnyArgs(new Exception("No such host is known"));

            var service = new R4FhirHealthService(fhirClient);
            var response = await service.CheckHealth();

            Assert.Equal(500, response.StatusCode);
            Assert.Equal("No such host is known", response.Message);
        }
    }
}
