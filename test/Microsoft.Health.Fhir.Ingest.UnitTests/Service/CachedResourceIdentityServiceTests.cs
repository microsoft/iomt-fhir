// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Data;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class CachedResourceIdentityServiceTests
    {
        [Fact]
        public async void GivenEmptyCache_WhenResolveResourceIdentitiesAsync_ThenCacheNotUsed_Test()
        {
            var ids = Substitute.For<IDictionary<ResourceType, string>>();

            var idSrv = Substitute.ForPartsOf<TestHarnessCachedResourceIdentityService>();
            idSrv.HarnessGetCacheKey(Arg.Any<IMeasurementGroup>()).Returns("key");
            idSrv.HarnessResolveResourceIdentitiesInternalAsync(Arg.Any<IMeasurementGroup>()).Returns(Task.FromResult(ids));
            var mg = Substitute.For<IMeasurementGroup>();

            var result = await idSrv.ResolveResourceIdentitiesAsync(mg);
            Assert.NotNull(result);
            Assert.Equal(ids, result);

            idSrv.Received(1).HarnessGetCacheKey(mg);
            await idSrv.Received(1).HarnessResolveResourceIdentitiesInternalAsync(mg);
        }

        [Fact]
        public async void GivenEmptyCacheThenSameKeyAccessed_WhenResolveResourceIdentitiesAsync_ThenCacheUsed_Test()
        {
            var ids = Substitute.For<IDictionary<ResourceType, string>>();

            var mg1 = Substitute.For<IMeasurementGroup>();
            var mg2 = Substitute.For<IMeasurementGroup>();

            var idSrv = Substitute.ForPartsOf<TestHarnessCachedResourceIdentityService>();
            idSrv.HarnessGetCacheKey(Arg.Is<IMeasurementGroup>(v => v == mg1)).Returns("1");
            idSrv.HarnessGetCacheKey(Arg.Is<IMeasurementGroup>(v => v == mg2)).Returns("1");

            idSrv.HarnessResolveResourceIdentitiesInternalAsync(Arg.Any<IMeasurementGroup>()).Returns(Task.FromResult(ids));

            var result1 = await idSrv.ResolveResourceIdentitiesAsync(mg1);
            Assert.NotNull(result1);
            Assert.Equal(ids, result1);

            var result2 = await idSrv.ResolveResourceIdentitiesAsync(mg2);
            Assert.NotNull(result2);
            Assert.Equal(ids, result2);

            Assert.Equal(result1, result2);

            idSrv.Received(1).HarnessGetCacheKey(mg1);
            await idSrv.Received(1).HarnessResolveResourceIdentitiesInternalAsync(mg1);

            idSrv.Received(1).HarnessGetCacheKey(mg2);
            await idSrv.DidNotReceive().HarnessResolveResourceIdentitiesInternalAsync(mg2);
        }

        [Fact]
        public async void GivenEmptyCacheThenDifferentKeyAccessed_WhenResolveResourceIdentitiesAsync_ThenCacheNotUsed_Test()
        {
            var ids = Substitute.For<IDictionary<ResourceType, string>>();

            var mg1 = Substitute.For<IMeasurementGroup>();
            var mg2 = Substitute.For<IMeasurementGroup>();

            var idSrv = Substitute.ForPartsOf<TestHarnessCachedResourceIdentityService>();
            idSrv.HarnessGetCacheKey(Arg.Is<IMeasurementGroup>(v => v == mg1)).Returns("1");
            idSrv.HarnessGetCacheKey(Arg.Is<IMeasurementGroup>(v => v == mg2)).Returns("2");
            idSrv.HarnessResolveResourceIdentitiesInternalAsync(Arg.Is<IMeasurementGroup>(v => v == mg2)).Returns(Task.FromResult(ids));

            var result1 = await idSrv.ResolveResourceIdentitiesAsync(mg1);
            var result2 = await idSrv.ResolveResourceIdentitiesAsync(mg2);
            Assert.NotNull(result2);
            Assert.Equal(ids, result2);

            idSrv.Received(1).HarnessGetCacheKey(mg2);
            await idSrv.Received(1).HarnessResolveResourceIdentitiesInternalAsync(mg2);
        }

        public abstract class TestHarnessCachedResourceIdentityService : CachedResourceIdentityService
        {
            public abstract string HarnessGetCacheKey(IMeasurementGroup input);

            public abstract Task<IDictionary<ResourceType, string>> HarnessResolveResourceIdentitiesInternalAsync(IMeasurementGroup input);

            protected override string GetCacheKey(IMeasurementGroup input)
            {
                return HarnessGetCacheKey(input);
            }

            protected async override Task<IDictionary<ResourceType, string>> ResolveResourceIdentitiesInternalAsync(IMeasurementGroup input)
            {
                return await HarnessResolveResourceIdentitiesInternalAsync(input);
            }
        }
    }
}
