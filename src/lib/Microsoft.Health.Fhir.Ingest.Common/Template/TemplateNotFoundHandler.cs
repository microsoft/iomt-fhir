// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Handler;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    internal class TemplateNotFoundHandler<TTemplate> : IResponsibilityHandler<TemplateContainer, TTemplate>
            where TTemplate : class
    {
        public TTemplate Evaluate(TemplateContainer request) => throw new InvalidTemplateException($"No match found for template type {request.TemplateType}.");
    }
}
