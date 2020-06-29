// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Ingest.Config;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class ResourceIdentityServiceAttribute : Attribute
    {
        public ResourceIdentityServiceAttribute(ResourceIdentityServiceType serviceType, Type classType)
        {
            ServiceType = serviceType;
            ClassType = classType;
        }

        public ResourceIdentityServiceType ServiceType { get; }

        public Type ClassType { get; }
    }
}
