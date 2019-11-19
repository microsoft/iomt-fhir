// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using EnsureThat;
using Microsoft.Health.Common;
using Microsoft.Health.Fhir.Ingest.Config;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class ResourceIdentityServiceFactory : IFactory<IResourceIdentityService, ResourceIdentityOptions>
    {
        private static readonly IFactory<IResourceIdentityService, ResourceIdentityOptions> _instance = new ResourceIdentityServiceFactory();

        private ResourceIdentityServiceFactory()
        {
        }

        public static IFactory<IResourceIdentityService, ResourceIdentityOptions> Instance => _instance;

        public IResourceIdentityService Create(ResourceIdentityOptions config, params object[] constructorParams)
        {
            EnsureArg.IsNotNull(config, nameof(config));

            var serviceType = Assembly
                .GetCallingAssembly()
                .GetTypes()
                .Where(t => typeof(IResourceIdentityService).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t.Name == config.ResourceIdentityServiceType)
                .FirstOrDefault();

            if (serviceType == null)
            {
                throw new NotSupportedException($"IResourceIdentityService type {config.ResourceIdentityServiceType} not found.");
            }

            var ctorParamTypes = constructorParams
                .Select(p => p.GetType())
                .ToArray();

            var ctor = serviceType.GetConstructor(ctorParamTypes);

            if (ctor == null)
            {
                throw new NotSupportedException($"IResourceIdentityService type {config.ResourceIdentityServiceType} does not support constructor parameter type(s) {string.Join<Type>(", ", ctorParamTypes)}");
            }

            var resourceIdentityService = (IResourceIdentityService)ctor.Invoke(constructorParams);
            resourceIdentityService.Initialize(config);

            return resourceIdentityService;
        }
    }
}
