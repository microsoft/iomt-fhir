// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Extensions.Fhir
{
    public struct ResourceMetadata : IResourceMetadata
    {
        public string Id { get; set; }

        public string VersionId { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
