// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Ingest.Host
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ResourceIdentityServiceAttribute : Attribute
    {
        public ResourceIdentityServiceAttribute(string type)
        {
            Type = type;
        }

        public string Type { get; }
    }
}
