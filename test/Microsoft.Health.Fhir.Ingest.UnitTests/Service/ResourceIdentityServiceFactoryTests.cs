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
using NSubstitute;
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
        public void GivenSupportedConfiguration_WhenLookup_ThenObjectCreated_Test()
        {
            var options = Substitute.For<ResourceIdentityOptions>();
            var identityService = Substitute.For<IResourceIdentityService>();
            identityService.GetResourceIdentityServiceType().ReturnsForAnyArgs(ResourceIdentityServiceType.Lookup);
            var resourceIdentityServiceFactory = new ResourceIdentityServiceFactory(new List<IResourceIdentityService>() { identityService }, options);
            var srv = resourceIdentityServiceFactory.Create();
            Assert.NotNull(srv);
        }

        [Fact]
        public void GivenSupportedConfiguration_WhenCreate_ThenObjectCreated_Test()
        {
            var options = Substitute.ForPartsOf<ResourceIdentityOptions>();
            options.ResourceIdentityServiceType = "Create";
            var identityService = Substitute.For<IResourceIdentityService>();
            identityService.GetResourceIdentityServiceType().ReturnsForAnyArgs(ResourceIdentityServiceType.Create);
            var resourceIdentityServiceFactory = new ResourceIdentityServiceFactory(new List<IResourceIdentityService>() { identityService }, options);
            var srv = resourceIdentityServiceFactory.Create();
            Assert.NotNull(srv);
        }

        // [Fact]
        public void GivenLegacySupportedConfiguration_WhenLookup_ThenObjectCreated_Test()
        {
            var options = Substitute.ForPartsOf<ResourceIdentityOptions>();
            options.ResourceIdentityServiceType = "R4DeviceAndPatientLookupIdentityService";
            var identityService = Substitute.For<IResourceIdentityService>();
            identityService.GetResourceIdentityServiceType().ReturnsForAnyArgs(ResourceIdentityServiceType.Lookup);
            var resourceIdentityServiceFactory = new ResourceIdentityServiceFactory(new List<IResourceIdentityService>() { identityService }, options);
            var srv = resourceIdentityServiceFactory.Create();
            Assert.NotNull(srv);
        }

        [Fact]
        public void GivenNotSupportedConfiguration_WhenLookupWithEncounter_ThenNotSupportedException_Test()
        {
            var options = Substitute.ForPartsOf<ResourceIdentityOptions>();
            options.ResourceIdentityServiceType = "LookupWithEncounter";
            var identityService = Substitute.For<IResourceIdentityService>();
            identityService.GetResourceIdentityServiceType().ReturnsForAnyArgs(ResourceIdentityServiceType.Lookup);
            var srv = new ResourceIdentityServiceFactory(new List<IResourceIdentityService>() { identityService }, options);
            var ex = Assert.Throws<ArgumentException>(() => srv.Create());
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void GivenSupportedConfigurationAndInvalidCtorParams_WhenCreate_ThenNotSupportedException_Test()
        {
            var options = Substitute.ForPartsOf<ResourceIdentityOptions>();
            options.ResourceIdentityServiceType = "Create";
            var identityService = Substitute.For<IResourceIdentityService>();
            identityService.GetResourceIdentityServiceType().ReturnsForAnyArgs(ResourceIdentityServiceType.Lookup);
            var srv = new ResourceIdentityServiceFactory(new List<IResourceIdentityService>() { identityService }, options);
            var ex = Assert.Throws<ArgumentException>(() => srv.Create());
            _output.WriteLine(ex.Message);
        }

        [ResourceIdentityService("Lookup")]
        [ResourceIdentityService("R4DeviceAndPatientLookupIdentityService")]
        public class TestLookupResourceIdentityService : IResourceIdentityService
        {
            public ResourceIdentityOptions Options { get; private set; }

            public ResourceIdentityServiceType GetResourceIdentityServiceType()
            {
                return ResourceIdentityServiceType.Lookup;
            }

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

            public ResourceIdentityServiceType GetResourceIdentityServiceType()
            {
                return ResourceIdentityServiceType.Create;
            }

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

            public ResourceIdentityServiceType GetResourceIdentityServiceType()
            {
                throw new NotImplementedException();
            }

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
