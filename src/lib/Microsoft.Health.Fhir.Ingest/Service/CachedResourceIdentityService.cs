// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public abstract class CachedResourceIdentityService :
        IResourceIdentityService,
        IDisposable
    {
        ~CachedResourceIdentityService()
        {
            Dispose(false);
        }

        protected IMemoryCache IdentityCache { get; private set; } = new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions { SizeLimit = 5000 }));

        protected ResourceIdentityOptions ResourceIdentityOptions { get; private set; }

        public async Task<IDictionary<ResourceType, string>> ResolveResourceIdentitiesAsync(IMeasurementGroup input)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            var cacheKey = GetCacheKey(input);

            return await IdentityCache.GetOrCreateAsync(
                cacheKey,
                async e =>
                {
                    e.SetSlidingExpiration(TimeSpan.FromHours(2));
                    e.Size = 1;
                    return await ResolveResourceIdentitiesInternalAsync(input).ConfigureAwait(false);
                })
            .ConfigureAwait(false);
        }

        public void Initialize(ResourceIdentityOptions options)
        {
            ResourceIdentityOptions = EnsureArg.IsNotNull(options);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IdentityCache.TryDispose())
                {
                    IdentityCache = null;
                }
            }
        }

        protected abstract Task<IDictionary<ResourceType, string>> ResolveResourceIdentitiesInternalAsync(IMeasurementGroup input);

        protected abstract string GetCacheKey(IMeasurementGroup input);
    }
}
