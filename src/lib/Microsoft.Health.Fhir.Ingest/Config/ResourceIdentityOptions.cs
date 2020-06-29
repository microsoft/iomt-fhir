// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Config
{
    public class ResourceIdentityOptions
    {
        public ResourceIdentityServiceType ResourceIdentityServiceType { get; set; } = ResourceIdentityServiceType.Lookup;

        public string DefaultDeviceIdentifierSystem { get; set; } = null;
    }
}
