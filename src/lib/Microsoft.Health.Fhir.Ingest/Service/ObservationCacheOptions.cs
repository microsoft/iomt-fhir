// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class ObservationCacheOptions : MemoryCacheOptions
    {
        public const string Settings = "ObservationCache";

        public ObservationCacheOptions()
        {
            SizeLimit = 5000;
        }
    }
}
