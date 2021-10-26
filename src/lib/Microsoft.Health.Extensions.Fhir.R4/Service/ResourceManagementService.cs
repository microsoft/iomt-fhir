// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Health.Extensions.Fhir.Repository;
using Microsoft.Health.Extensions.Fhir.Search;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Extensions.Fhir.Service
{
    public class ResourceManagementService
    {
        private readonly IFhirServerRepository _fhirServerRepository;

        public ResourceManagementService(IFhirServerRepository fhirServerRepository)
        {
            _fhirServerRepository = EnsureArg.IsNotNull(fhirServerRepository, nameof(fhirServerRepository));
        }

        /// <summary>
        /// Gets or creates the FHIR Resource with the provided identifier.
        /// </summary>
        /// <typeparam name="TResource">The type of FHIR resource to ensure exists.</typeparam>
        /// <param name="client">Client to use for FHIR rest calls.</param>
        /// <param name="value">The identifier value to search for or create.</param>
        /// <param name="system">The system the identifier belongs to.</param>
        /// <param name="propertySetter">Optional setter to provide property values if the resource needs to be created.</param>
        /// <returns>Reource that was found or created.</returns>
        public virtual async Task<TResource> EnsureResourceByIdentityAsync<TResource>(FhirClient client, string value, string system, Action<TResource, Model.Identifier> propertySetter = null)
            where TResource : Model.Resource, new()
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNullOrWhiteSpace(value, nameof(value));

            var identifier = BuildIdentifier(value, system);
            return await GetResourceByIdentityAsync<TResource>(client, identifier).ConfigureAwait(false)
                ?? await CreateResourceByIdentityAsync(client, identifier, propertySetter).ConfigureAwait(false);
        }

        public virtual async Task<TResource> GetResourceByIdentityAsync<TResource>(FhirClient client, string value, string system)
            where TResource : Model.Resource, new()
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNullOrWhiteSpace(value, nameof(value));

            var identifier = BuildIdentifier(value, system);
            return await GetResourceByIdentityAsync<TResource>(client, identifier).ConfigureAwait(false);
        }

        protected async Task<TResource> GetResourceByIdentityAsync<TResource>(FhirClient client, Model.Identifier identifier)
            where TResource : Model.Resource, new()
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNull(identifier, nameof(identifier));
            var searchParams = identifier.ToSearchParams();
            var result = await _fhirServerRepository.SearchForResourceAsync<TResource>(searchParams).ConfigureAwait(false);
            return await result.ReadOneFromBundleWithContinuationAsync<TResource>(client);
        }

        protected static async Task<TResource> CreateResourceByIdentityAsync<TResource>(FhirClient client, Model.Identifier identifier, Action<TResource, Model.Identifier> propertySetter)
            where TResource : Model.Resource, new()
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNull(identifier, nameof(identifier));
            var resource = new TResource();

            propertySetter?.Invoke(resource, identifier);

            return await client.CreateAsync<TResource>(resource).ConfigureAwait(false);
        }

        private static Model.Identifier BuildIdentifier(string value, string system)
        {
            var identifier = new Model.Identifier { Value = value, System = string.IsNullOrWhiteSpace(system) ? null : system };
            return identifier;
        }
    }
}