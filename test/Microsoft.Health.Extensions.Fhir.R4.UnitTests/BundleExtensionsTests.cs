﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Health.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Extensions.Fhir.R4.UnitTests
{
    public class BundleExtensionsTests
    {
        private static Uri NextBundleLink { get; } = new Uri("https://localhost/next");

        [Fact]
        public async void GivenNoEntriesAndNoContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenNullIsReturned_Test()
        {
            var bundle = new Bundle
            {
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
            };

            var client = Utilities.CreateMockFhirClient();

            Assert.Null(await bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client));
        }

        [Fact]
        public async void GivenOneEntryAndNoContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenResourceIsReturned_Test()
        {
            var bundle = new Bundle
            {
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
            };

            var observation = new Observation
            {
                Id = Guid.NewGuid().ToString(),
            };
            var entry = new Bundle.EntryComponent
            {
                Resource = observation,
            };
            bundle.Entry.Add(entry);

            MockFhirResourceHttpMessageHandler messageHandler = Utilities.CreateMockMessageHandler();
            messageHandler.GetReturnContent(default).ReturnsForAnyArgs((Bundle)null);

            var client = Utilities.CreateMockFhirClient(messageHandler);

            var result = await bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client);

            Assert.NotNull(result);
            Assert.Equal(observation.ToJson(), result.ToJson());
        }

        [Fact]
        public async void GivenOneEntryAfterContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenResourceIsReturned_Test()
        {
            var bundle = new Bundle
            {
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
                NextLink = NextBundleLink,
            };

            var continuationBundle = new Bundle
            {
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
            };

            var observation = new Observation
            {
                Id = Guid.NewGuid().ToString(),
            };

            var entry = new Bundle.EntryComponent
            {
                Resource = observation,
            };
            continuationBundle.Entry.Add(entry);

            MockFhirResourceHttpMessageHandler messageHandler = Utilities.CreateMockMessageHandler();
            messageHandler.GetReturnContent(default).ReturnsForAnyArgs(continuationBundle, null);
            var client = Utilities.CreateMockFhirClient(messageHandler);

            var result = await bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client);

            Assert.NotNull(result);
            Assert.Equal(observation.ToJson(), result.ToJson());
        }

        [Fact]
        public async void GivenTwoEntriesAndNoContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenThrows_Test()
        {
            var bundle = new Bundle
            {
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
            };

            var observation1 = new Observation();
            var entry1 = new Bundle.EntryComponent
            {
                Resource = observation1,
            };
            bundle.Entry.Add(entry1);

            var observation2 = new Observation();
            var entry2 = new Bundle.EntryComponent
            {
                Resource = observation2,
            };
            bundle.Entry.Add(entry2);

            var client = Utilities.CreateMockFhirClient();

            await Assert.ThrowsAsync<MultipleResourceFoundException<Observation>>(() => bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client));
        }

        [Fact]
        public async void GivenOneEntryBeforeAndAfterContinuationToken_ReadOneFromBundleWithContinuationAsync_ThenThrows_Test()
        {
            var bundle = new Bundle
            {
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
                NextLink = NextBundleLink,
            };

            var observation1 = new Observation
            {
                Id = "1",
            };
            var entry1 = new Bundle.EntryComponent
            {
                Resource = observation1,
            };
            bundle.Entry.Add(entry1);

            var continuationBundle = new Bundle
            {
                Entry = new List<Bundle.EntryComponent>(),
                Link = new List<Bundle.LinkComponent>(),
            };

            var observation2 = new Observation
            {
                Id = "2",
            };
            var entry2 = new Bundle.EntryComponent
            {
                Resource = observation2,
            };
            continuationBundle.Entry.Add(entry2);

            MockFhirResourceHttpMessageHandler messageHandler = Utilities.CreateMockMessageHandler();
            messageHandler.GetReturnContent(default).ReturnsForAnyArgs(continuationBundle, null);
            var client = Utilities.CreateMockFhirClient(messageHandler);

            await Assert.ThrowsAsync<MultipleResourceFoundException<Observation>>(() => bundle.ReadOneFromBundleWithContinuationAsync<Observation>(client));
        }
    }
}
