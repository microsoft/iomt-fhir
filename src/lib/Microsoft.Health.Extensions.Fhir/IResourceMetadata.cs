// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Extensions.Fhir
{
    public interface IResourceMetadata
    {
        string Id { get; }

        string VersionId { get;  }

        DateTime? LastUpdated { get; }
    }
}
