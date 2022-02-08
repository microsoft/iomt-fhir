// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

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
            var client = Substitute.For<IFhirClient>();
            client.ReadAsync<Hl7.Fhir.Model.CapabilityStatement>(Arg.Any<string>()).ThrowsForAnyArgs(new HttpRequestException());

            var logger = Substitute.For<ITelemetryLogger>();

            bool actualIsValid = await FhirServiceValidator.ValidateFhirServiceAsync(client, url, logger);

            Assert.Equal(expectedIsValid, actualIsValid);
        }
    }
}
