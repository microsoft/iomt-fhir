// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Host;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Health.Fhir.Ingest.Service.ResourceIdentityServiceFactoryTests;

[assembly: ResourceIdentityService(ResourceIdentityServiceType.Lookup, typeof(TestLookUpResourceIdentityService))]
[assembly: ResourceIdentityService(ResourceIdentityServiceType.Create, typeof(TestCreateResourceIdentityService))]

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class ResourceIdentityServiceFactoryTests
    {
        private readonly ITestOutputHelper _output;

        public ResourceIdentityServiceFactoryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GivenSupportedConfigurationAndNoCtorParams_WhenLookUp_ThenObjectCreated_Test()
        {
            var options = new ResourceIdentityOptions { ResourceIdentityServiceType = ResourceIdentityServiceType.Lookup };
            var srv = ResourceIdentityServiceFactory.Instance.Create(options);
            Assert.NotNull(srv);
            var typedSrv = Assert.IsType<TestLookUpResourceIdentityService>(srv);
            Assert.Equal(options, typedSrv.Options);
        }

        [Fact]
        public void GivenSupportedConfigurationAndValidCtorParams_WhenCreate_ThenObjectCreatedWithParam_Test()
        {
            var options = new ResourceIdentityOptions { ResourceIdentityServiceType = ResourceIdentityServiceType.Create };
            var srv = ResourceIdentityServiceFactory.Instance.Create(options, "foo");
            Assert.NotNull(srv);
            var typedSrv = Assert.IsType<TestCreateResourceIdentityService>(srv);
            Assert.Equal("foo", typedSrv.Parameter);
            Assert.Equal(options, typedSrv.Options);
        }

        [Fact]
        public void GivenNotSupportedConfigurationAndInvalidCtorParams_WhenLookUpWithEncounter_ThenNotSupportedException_Test()
        {
            var ex = Assert.Throws<NotSupportedException>(() => ResourceIdentityServiceFactory.Instance.Create(new ResourceIdentityOptions { ResourceIdentityServiceType = ResourceIdentityServiceType.LookupWithEncounter }));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void GivenSupportedConfigurationAndInvalidCtorParams_WhenCreate_ThenNotSupportedException_Test()
        {
           var ex = Assert.Throws<NotSupportedException>(() => ResourceIdentityServiceFactory.Instance.Create(new ResourceIdentityOptions { ResourceIdentityServiceType = ResourceIdentityServiceType.Create }, 1));
           _output.WriteLine(ex.Message);
        }

        public class TestLookUpResourceIdentityService : IResourceIdentityService
        {
            public ResourceIdentityOptions Options { get; private set; }

            public void Initialize(ResourceIdentityOptions options)
            {
                Options = options;
            }

            public Task<IDictionary<ResourceType, string>> ResolveResourceIdentitiesAsync(IMeasurementGroup input)
            {
                throw new NotImplementedException();
            }
        }

        public class TestCreateResourceIdentityService : IResourceIdentityService
        {
            public TestCreateResourceIdentityService(string parameter)
            {
                Parameter = parameter;
            }

            public string Parameter { get; private set; }

            public ResourceIdentityOptions Options { get; private set; }

            public void Initialize(ResourceIdentityOptions options)
            {
                Options = options;
            }

            public Task<IDictionary<ResourceType, string>> ResolveResourceIdentitiesAsync(IMeasurementGroup input)
            {
                throw new NotImplementedException();
            }
        }
    }
}
