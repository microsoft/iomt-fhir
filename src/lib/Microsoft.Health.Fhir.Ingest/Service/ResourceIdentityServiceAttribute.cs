// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Ingest.Config;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ResourceIdentityServiceAttribute : Attribute
    {
        public ResourceIdentityServiceAttribute(ResourceIdentityServiceType type)
        {
            Type = type;
        }

        public ResourceIdentityServiceType Type { get; }
    }
}
