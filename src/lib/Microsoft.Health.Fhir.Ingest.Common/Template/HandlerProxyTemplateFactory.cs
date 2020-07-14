// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Common.Handler;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public abstract class HandlerProxyTemplateFactory<TInput, TOutput> :
        ITemplateFactory<TInput, TOutput>,
        IResponsibilityHandler<TInput, TOutput>
        where TOutput : class
    {
        public abstract TOutput Create(TInput jsonTemplate);

        TOutput IResponsibilityHandler<TInput, TOutput>.Evaluate(TInput request) => Create(request);
    }
}
