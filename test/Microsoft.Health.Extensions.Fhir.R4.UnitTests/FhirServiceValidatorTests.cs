// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Client;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Microsoft.Health.Extensions.Fhir.R4.UnitTests
{
    public class FhirServiceValidatorTests
    {
        [Theory]
        [InlineData("https://testfoobar.azurehealthcareapis.com")]
        [InlineData("https://microsoft.com")]
        public async Task GivenInvalidFhirServiceUrl_WhenValidateFhirService_ThenNotValidReturned_Test(string url)
        {
            await ValidateFhirServiceUrl(url, false);
        }

        private async Task ValidateFhirServiceUrl(string url, bool expectedIsValid)
        {
            var client = Substitute.For<HttpClient>();

            var fhirClient = Substitute.ForPartsOf<FhirClient>(client);

            string uri = url + "/metadata";

            fhirClient.ReadAsync<Hl7.Fhir.Model.CapabilityStatement>(uri).ThrowsForAnyArgs(new Exception());

            var logger = Substitute.For<ITelemetryLogger>();

            bool actualIsValid = await FhirClientValidator.ValidateFhirClientAsync(client, logger);

            Assert.Equal(expectedIsValid, actualIsValid);
        }
    }
}
