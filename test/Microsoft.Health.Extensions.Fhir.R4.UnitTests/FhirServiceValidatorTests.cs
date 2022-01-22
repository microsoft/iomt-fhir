// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Hl7.Fhir.Rest;
using Microsoft.Health.Logging.Telemetry;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Extensions.Fhir.R4.UnitTests
{
    public class FhirServiceValidatorTests
    {
        [Theory]
        [InlineData("https://testfoobar.azurehealthcareapis.com")]
        [InlineData("https://microsoft.com")]
        public void GivenInvalidFhirServiceUrl_WhenValidateFhirService_ThenNotValidReturned_Test(string url)
        {
            ValidateFhirServiceUrl(url, false);
        }

        private void ValidateFhirServiceUrl(string url, bool expectedIsValid)
        {
            var fhirClientSettings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
            };

            using (var client = new FhirClient(url, fhirClientSettings))
            {
                var logger = Substitute.For<ITelemetryLogger>();

                bool actualIsValid = FhirServiceValidator.ValidateFhirService(client, logger);

                Assert.Equal(expectedIsValid, actualIsValid);
            }
        }
    }
}
