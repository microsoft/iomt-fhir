// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Config;
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

        [Fact]
        public void GivenMismatchedSupportedConfigurations_WhenLookupWithEncounter_ThenArguementException_Test()
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
        public void GivenMismatchedSupportedConfiguration_WhenCreate_ThenArguementException_Test()
        {
            var options = Substitute.ForPartsOf<ResourceIdentityOptions>();
            options.ResourceIdentityServiceType = "Create";
            var identityService = Substitute.For<IResourceIdentityService>();
            identityService.GetResourceIdentityServiceType().ReturnsForAnyArgs(ResourceIdentityServiceType.Lookup);
            var srv = new ResourceIdentityServiceFactory(new List<IResourceIdentityService>() { identityService }, options);
            var ex = Assert.Throws<ArgumentException>(() => srv.Create());
            _output.WriteLine(ex.Message);
        }
    }
}
