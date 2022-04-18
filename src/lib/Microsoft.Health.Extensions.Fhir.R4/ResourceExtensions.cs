// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class ResourceExtensions
    {
        /// <summary>
        /// Performs full deep copy of the resource.
        /// </summary>
        /// <typeparam name="TResource">Type of resource to return.</typeparam>
        /// <param name="resource">Resource to copy.</param>
        /// <returns>New resource object with the contents of the original.</returns>
        public static TResource FullCopy<TResource>(this TResource resource)
            where TResource : class, IDeepCopyable
        {
            EnsureArg.IsNotNull(resource, nameof(resource));

            return resource.DeepCopy() as TResource;
        }
    }
}
