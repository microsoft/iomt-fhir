// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Common;
using Microsoft.Health.Fhir.Ingest.Config;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class ResourceIdentityServiceFactory : IFactory<IResourceIdentityService>
    {
        private readonly IDictionary<string, IResourceIdentityService> _resourceIdentityServices;
        private readonly ResourceIdentityOptions config;

        public ResourceIdentityServiceFactory(
            IList<IResourceIdentityService> resourceIdentityServices,
            ResourceIdentityOptions resourceIdentityOptions)
        {
            EnsureArg.IsNotNull(resourceIdentityServices);
            config = EnsureArg.IsNotNull(resourceIdentityOptions, nameof(resourceIdentityOptions));
            _resourceIdentityServices = resourceIdentityServices.ToDictionary((service) => service.GetResourceIdentityServiceType().ToString());
        }

        public IResourceIdentityService Create()
        {
            if (!_resourceIdentityServices.TryGetValue(config.ResourceIdentityServiceType, out IResourceIdentityService resourceIdentityService))
            {
                throw new ArgumentException($"Unsupported Resource Identity Service Type {config.ResourceIdentityServiceType}");
            }

            resourceIdentityService.Initialize(config);

            return resourceIdentityService;
        }
    }
}
