// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnsureThat;
using Microsoft.Health.Common;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Host;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class ResourceIdentityServiceFactory : IFactory<IResourceIdentityService, ResourceIdentityOptions>
    {
        private static readonly IDictionary<ResourceIdentityServiceType, Type> _identityServiceRegistry = GetResourceIdentityServiceRegistry();

        private ResourceIdentityServiceFactory()
        {
        }

        public static IFactory<IResourceIdentityService, ResourceIdentityOptions> Instance { get; } = new ResourceIdentityServiceFactory();

        public IResourceIdentityService Create(ResourceIdentityOptions config, params object[] constructorParams)
        {
            EnsureArg.IsNotNull(config, nameof(config));

            if (!_identityServiceRegistry.TryGetValue(config.ResourceIdentityServiceType, out Type serviceClassType))
            {
                throw new NotSupportedException($"IResourceIdentityService type {config.ResourceIdentityServiceType} not found.");
            }

            var ctorParamTypes = constructorParams
                .Select(p => p.GetType())
                .ToArray();

            var ctor = serviceClassType.GetConstructor(ctorParamTypes);

            if (ctor == null)
            {
                throw new NotSupportedException($"IResourceIdentityService type {config.ResourceIdentityServiceType} does not support constructor parameter type(s) {string.Join<Type>(", ", ctorParamTypes)}");
            }

            var resourceIdentityService = (IResourceIdentityService)ctor.Invoke(constructorParams);
            resourceIdentityService.Initialize(config);

            return resourceIdentityService;
        }

        /// <summary>
        /// Returns the registry of resource identity service classes. The class needs to be declared with the ResourceIdentityServiceAttribute
        /// explicitly and built in the assembly. There should be only one service class registered for each ResourceIdentityServiceType. The
        /// dynamic types will not get loaded.
        /// </summary>
        /// <returns>The registry of resource identity service class types.</returns>
        private static IDictionary<ResourceIdentityServiceType, Type> GetResourceIdentityServiceRegistry()
        {
            IDictionary<ResourceIdentityServiceType, Type> serviceTypeRegistry = new Dictionary<ResourceIdentityServiceType, Type>();
            AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .ToList()
                .ForEach(assembly =>
                {
                    foreach (Type classType in assembly.GetTypes())
                    {
                        if (typeof(IResourceIdentityService).IsAssignableFrom(classType) && classType.IsClass && !classType.IsAbstract)
                        {
                            ResourceIdentityServiceAttribute attribute = classType.GetCustomAttribute(typeof(ResourceIdentityServiceAttribute), false) as ResourceIdentityServiceAttribute;
                            if (attribute == null)
                            {
                                continue;
                            }

                            if (!serviceTypeRegistry.TryGetValue(attribute.Type, out Type existClassType))
                            {
                                serviceTypeRegistry.Add(attribute.Type, classType);
                            }
                            else
                            {
                                throw new TypeLoadException($"Duplicate class types found for IResourceIdentityService type '{attribute.Type}': '{existClassType.FullName}', '{classType.FullName}'.");
                            }
                        }
                    }
                });

            return serviceTypeRegistry;
        }
    }
}
