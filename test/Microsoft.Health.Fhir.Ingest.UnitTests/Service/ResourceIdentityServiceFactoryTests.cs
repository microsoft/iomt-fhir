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
        public void GivenSupportedConfigurationAndNoCtorParams_WhenLookup_ThenObjectCreated_Test()
        {
            var options = new ResourceIdentityOptions { ResourceIdentityServiceType = "Lookup" };
            var srv = ResourceIdentityServiceFactory.Instance.Create(options);
            Assert.NotNull(srv);
            var typedSrv = Assert.IsType<TestLookupResourceIdentityService>(srv);
            Assert.Equal(options, typedSrv.Options);
        }

        [Fact]
        public void GivenSupportedConfigurationAndValidCtorParams_WhenCreate_ThenObjectCreatedWithParam_Test()
        {
            var options = new ResourceIdentityOptions { ResourceIdentityServiceType = "Create" };
            var srv = ResourceIdentityServiceFactory.Instance.Create(options, "foo");
            Assert.NotNull(srv);
            var typedSrv = Assert.IsType<TestCreateResourceIdentityService>(srv);
            Assert.Equal("foo", typedSrv.Parameter);
            Assert.Equal(options, typedSrv.Options);
        }

        [Fact]
        public void GivenLegacySupportedConfigurationAndNoCtorParams_WhenLookup_ThenObjectCreated_Test()
        {
            var options = new ResourceIdentityOptions { ResourceIdentityServiceType = "R4DeviceAndPatientLookupIdentityService" };
            var srv = ResourceIdentityServiceFactory.Instance.Create(options);
            Assert.NotNull(srv);
            var typedSrv = Assert.IsType<TestLookupResourceIdentityService>(srv);
            Assert.Equal(options, typedSrv.Options);
        }

        [Fact]
        public void GivenNotSupportedConfigurationAndInvalidCtorParams_WhenLookupWithEncounter_ThenNotSupportedException_Test()
        {
            var ex = Assert.Throws<NotSupportedException>(() => ResourceIdentityServiceFactory.Instance.Create(new ResourceIdentityOptions { ResourceIdentityServiceType = "LookupWithEncounter" }));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void GivenSupportedConfigurationAndInvalidCtorParams_WhenCreate_ThenNotSupportedException_Test()
        {
            var ex = Assert.Throws<NotSupportedException>(() => ResourceIdentityServiceFactory.Instance.Create(new ResourceIdentityOptions { ResourceIdentityServiceType = "Create" }, 1));
            _output.WriteLine(ex.Message);
        }

        [ResourceIdentityService("Lookup")]
        [ResourceIdentityService("R4DeviceAndPatientLookupIdentityService")]
        public class TestLookupResourceIdentityService : IResourceIdentityService
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

        [ResourceIdentityService("Create")]
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

        public class TestUnregisteredResourceIdentityService : IResourceIdentityService
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
    }
}
