// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public interface ITemplateContext<TTemplate> : IValidatableObject
    {
        TTemplate Template { get; }
    }
}
