// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Hl7.Fhir.Model;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Client;
using NSubstitute;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.Extensions.Fhir.R4.UnitTests
{
    public class FhirServiceTests
    {
        private IFhirService _fhirService;
        private IFhirClient _mockFhirClient;

        public FhirServiceTests()
        {
            _mockFhirClient = Substitute.For<IFhirClient>();
            _fhirService = new FhirService(_mockFhirClient);
        }

        [Fact]
        public async void GivenBundleWithContinuation_WhenIterateOverAdditionalBundlesInvoked_ThenAllAdditionalBundlesReturned()
        {
            var bundle0NextLink = new Uri("https://fhir/bundle1");
            var bundle0 = new Bundle
            {
                Id = "0",
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
                NextLink = bundle0NextLink,
            };

            var bundle1NextLink = new Uri("https://fhir/bundle2");
            var bundle1 = new Bundle
            {
                Id = "1",
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
                NextLink = bundle1NextLink,
            };

            var bundle2NextLink = new Uri("https://fhir/bundle3");
            var bundle2 = new Bundle
            {
                Id = "2",
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
                NextLink = bundle2NextLink,
            };

            var bundle3 = new Bundle
            {
                Id = "3",
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
            };

            _mockFhirClient.SearchAsync(bundle0NextLink.ToString())
                .Returns(Task.FromResult(new FhirResponse<Bundle>(new HttpResponseMessage(HttpStatusCode.OK), bundle1)));

            _mockFhirClient.SearchAsync(bundle1NextLink.ToString())
                .Returns(Task.FromResult(new FhirResponse<Bundle>(new HttpResponseMessage(HttpStatusCode.OK), bundle2)));

            _mockFhirClient.SearchAsync(bundle2NextLink.ToString())
                .Returns(Task.FromResult(new FhirResponse<Bundle>(new HttpResponseMessage(HttpStatusCode.OK), bundle3)));

            var bundleCount = 1;
            await foreach (var bundle in _fhirService.IterateOverAdditionalBundlesAsync(bundle0))
            {
                var currentBundle = bundle;
                Assert.Equal(bundleCount++, int.Parse(currentBundle.Id));
            }

            Assert.Equal(4, bundleCount);

            await _mockFhirClient.Received(1).SearchAsync(bundle0NextLink.ToString());
            await _mockFhirClient.Received(1).SearchAsync(bundle1NextLink.ToString());
            await _mockFhirClient.Received(1).SearchAsync(bundle2NextLink.ToString());
        }

        [Fact]
        public async void GivenBundleWithNoContinuation_WhenIterateOverAdditionalBundlesInvoked_ThenNoAdditionalBundleReturned()
        {
            var bundle0 = new Bundle
            {
                Id = "0",
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
            };

            await foreach (var bundle in _fhirService.IterateOverAdditionalBundlesAsync(bundle0))
            {
                Assert.Fail("Unexpected result. No additional bundles should be returned.");
            }

            await _mockFhirClient.Received(0).SearchAsync(Arg.Any<string>());
        }
    }
}
