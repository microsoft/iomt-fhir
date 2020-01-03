// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Extensions.Fhir.R4.UnitTests
{
    public class BundleExtensionsTests
    {
        [Fact]
        public async void GivenNoEntriesAndNoContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenNullIsReturned_Test()
        {
            var bundle = new Bundle();
            bundle.Entry = new List<Bundle.EntryComponent>();
            bundle.Link = new List<Bundle.LinkComponent>();

            var client = Substitute.For<IFhirClient>();
            client.ContinueAsync(Arg.Any<Bundle>()).Returns(System.Threading.Tasks.Task.FromResult<Bundle>(null));

            Assert.Null(await bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client));
        }

        [Fact]
        public async void GivenOneEntryAndNoContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenResourceIsReturned_Test()
        {
            var bundle = new Bundle();
            bundle.Entry = new List<Bundle.EntryComponent>();
            bundle.Link = new List<Bundle.LinkComponent>();

            var observation = new Observation();
            var entry = new Bundle.EntryComponent();
            entry.Resource = observation;
            bundle.Entry.Add(entry);

            var client = Substitute.For<IFhirClient>();
            client.ContinueAsync(Arg.Any<Bundle>()).Returns(System.Threading.Tasks.Task.FromResult<Bundle>(null));

            Assert.Equal(observation, await bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client));
        }

        [Fact]
        public async void GivenOneEntryAfterContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenResourceIsReturned_Test()
        {
            var bundle = new Bundle();
            bundle.Entry = new List<Bundle.EntryComponent>();
            bundle.Link = new List<Bundle.LinkComponent>();

            var continuationBundle = new Bundle();
            continuationBundle.Entry = new List<Bundle.EntryComponent>();
            continuationBundle.Link = new List<Bundle.LinkComponent>();

            var observation = new Observation();
            var entry = new Bundle.EntryComponent();
            entry.Resource = observation;
            continuationBundle.Entry.Add(entry);

            var client = Substitute.For<IFhirClient>();
            client.ContinueAsync(Arg.Any<Bundle>()).Returns(ret => System.Threading.Tasks.Task.FromResult(continuationBundle), ret => System.Threading.Tasks.Task.FromResult<Bundle>(null));

            Assert.Equal(observation, await bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client));
        }

        [Fact]
        public async void GivenTwoEntriesAndNoContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenThrows_Test()
        {
            var bundle = new Bundle();
            bundle.Entry = new List<Bundle.EntryComponent>();
            bundle.Link = new List<Bundle.LinkComponent>();

            var observation1 = new Observation();
            var entry1 = new Bundle.EntryComponent();
            entry1.Resource = observation1;
            bundle.Entry.Add(entry1);

            var observation2 = new Observation();
            var entry2 = new Bundle.EntryComponent();
            entry2.Resource = observation2;
            bundle.Entry.Add(entry2);

            var client = Substitute.For<IFhirClient>();
            client.ContinueAsync(Arg.Any<Bundle>()).Returns(System.Threading.Tasks.Task.FromResult<Bundle>(null));

            await Assert.ThrowsAsync<MultipleResourceFoundException<Observation>>(() => bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client));
        }

        [Fact]
        public async void GivenOneEntryBeforeAndAfterContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenThrows_Test()
        {
            var bundle = new Bundle();
            bundle.Entry = new List<Bundle.EntryComponent>();
            bundle.Link = new List<Bundle.LinkComponent>();

            var observation1 = new Observation();
            var entry1 = new Bundle.EntryComponent();
            entry1.Resource = observation1;
            bundle.Entry.Add(entry1);

            var continuationBundle = new Bundle();
            continuationBundle.Entry = new List<Bundle.EntryComponent>();
            continuationBundle.Link = new List<Bundle.LinkComponent>();

            var observation2 = new Observation();
            var entry2 = new Bundle.EntryComponent();
            entry2.Resource = observation2;
            continuationBundle.Entry.Add(entry2);

            var client = Substitute.For<IFhirClient>();
            client.ContinueAsync(Arg.Any<Bundle>()).Returns(ret => System.Threading.Tasks.Task.FromResult(continuationBundle), ret => System.Threading.Tasks.Task.FromResult<Bundle>(null));

            await Assert.ThrowsAsync<MultipleResourceFoundException<Observation>>(() => bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client));
        }
    }
}
