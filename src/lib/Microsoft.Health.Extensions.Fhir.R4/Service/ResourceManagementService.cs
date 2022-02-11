// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Model;
using Microsoft.Health.Extensions.Fhir.Search;
using Model = Hl7.Fhir.Model;

namespace Microsoft.Health.Extensions.Fhir.Service
{
    public class ResourceManagementService
    {
        public ResourceManagementService(IFhirService fhirService)
        {
            FhirService = EnsureArg.IsNotNull(fhirService, nameof(fhirService));
        }

        public IFhirService FhirService { get; private set; }

        /// <summary>
        /// Gets or creates the FHIR Resource with the provided identifier.
        /// </summary>
        /// <typeparam name="TResource">The type of FHIR resource to ensure exists.</typeparam>
        /// <param name="value">The identifier value to search for or create.</param>
        /// <param name="system">The system the identifier belongs to.</param>
        /// <param name="propertySetter">Optional setter to provide property values if the resource needs to be created.</param>
        /// <returns>Reource that was found or created.</returns>
        public virtual async Task<TResource> EnsureResourceByIdentityAsync<TResource>(string value, string system, Action<TResource, Model.Identifier> propertySetter = null)
            where TResource : Model.Resource, new()
        {
            EnsureArg.IsNotNullOrWhiteSpace(value, nameof(value));

            var identifier = BuildIdentifier(value, system);
            return await GetResourceByIdentityAsync<TResource>(identifier).ConfigureAwait(false)
                ?? await CreateResourceByIdentityAsync(identifier, propertySetter).ConfigureAwait(false);
        }

        public virtual async Task<TResource> GetResourceByIdentityAsync<TResource>(string value, string system)
            where TResource : Model.Resource, new()
        {
            EnsureArg.IsNotNullOrWhiteSpace(value, nameof(value));

            var identifier = BuildIdentifier(value, system);
            return await GetResourceByIdentityAsync<TResource>(identifier).ConfigureAwait(false);
        }

        protected async Task<TResource> GetResourceByIdentityAsync<TResource>(Model.Identifier identifier)
            where TResource : Model.Resource, new()
        {
            EnsureArg.IsNotNull(identifier, nameof(identifier));

            string fhirTypeName = ModelInfo.GetFhirTypeNameForType(typeof(TResource));

            _ = Enum.TryParse(fhirTypeName, out ResourceType resourceType);

            Model.Bundle result = await FhirService.SearchForResourceAsync(resourceType, identifier.ToSearchToken()).ConfigureAwait(false);
            return await result.ReadOneFromBundleWithContinuationAsync<TResource>(FhirService);
        }

        protected async Task<TResource> CreateResourceByIdentityAsync<TResource>(Model.Identifier identifier, Action<TResource, Model.Identifier> propertySetter)
            where TResource : Model.Resource, new()
        {
            EnsureArg.IsNotNull(identifier, nameof(identifier));
            var resource = new TResource();

            propertySetter?.Invoke(resource, identifier);

            return await FhirService.CreateResourceAsync(resource).ConfigureAwait(false);
        }

        private static Model.Identifier BuildIdentifier(string value, string system)
        {
            var identifier = new Model.Identifier { Value = value, System = string.IsNullOrWhiteSpace(system) ? null : system };
            return identifier;
        }
    }
}