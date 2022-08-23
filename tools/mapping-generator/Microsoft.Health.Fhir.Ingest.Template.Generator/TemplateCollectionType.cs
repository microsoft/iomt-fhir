// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Template.Generator
{
    public enum TemplateCollectionType
    {
        /// <summary>
        /// A collection of mapping templates used to process device payload contents.
        /// </summary>
        CollectionContent,

        /// <summary>
        /// A collection of mapping templates used to process measurements to FHIR Resources.
        /// </summary>
        CollectionFhir,
    }
}
