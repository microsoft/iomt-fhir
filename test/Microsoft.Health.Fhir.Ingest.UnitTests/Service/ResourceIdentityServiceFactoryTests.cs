// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Xunit;
using Xunit.Abstractions;

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
        public void GivenValidConfigurationAndNoCtorParams_WhenCreate_ThenObjectCreated_Test()
        {
            var options = new ResourceIdentityOptions { ResourceIdentityServiceType = nameof(Test1ResourceIdentityService) };
            var srv = ResourceIdentityServiceFactory.Instance.Create(options);
            Assert.NotNull(srv);
            var typedSrv = Assert.IsType<Test1ResourceIdentityService>(srv);
            Assert.Equal(options, typedSrv.Options);
        }

        [Fact]
        public void GivenValidConfigurationAndCtorParams_WhenCreate_ThenObjectCreatedWithParam_Test()
        {
            var options = new ResourceIdentityOptions { ResourceIdentityServiceType = nameof(Test2ResourceIdentityService) };
            var srv = ResourceIdentityServiceFactory.Instance.Create(options, "foo");
            Assert.NotNull(srv);
            var typedSrv = Assert.IsType<Test2ResourceIdentityService>(srv);
            Assert.Equal("foo", typedSrv.Parameter);
            Assert.Equal(options, typedSrv.Options);
        }

        [Fact]
        public void GivenInValidConfigurationAndCtorParams_WhenCreate_ThenNotSupportedException_Test()
        {
            var ex = Assert.Throws<NotSupportedException>(() => ResourceIdentityServiceFactory.Instance.Create(new ResourceIdentityOptions { ResourceIdentityServiceType = "type1" }));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void GivenValidConfigurationAndInvalidCtorParams_WhenCreate_ThenNotSupportedException_Test()
        {
           var ex = Assert.Throws<NotSupportedException>(() => ResourceIdentityServiceFactory.Instance.Create(new ResourceIdentityOptions { ResourceIdentityServiceType = nameof(Test2ResourceIdentityService) }, 1));
           _output.WriteLine(ex.Message);
        }

        public class Test1ResourceIdentityService : IResourceIdentityService
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

        public class Test2ResourceIdentityService : IResourceIdentityService
        {
            public Test2ResourceIdentityService(string parameter)
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
