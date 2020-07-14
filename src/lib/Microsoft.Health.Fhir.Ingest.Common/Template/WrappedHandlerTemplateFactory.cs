// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public sealed class WrappedHandlerTemplateFactory<TInput, TOutput> : HandlerProxyTemplateFactory<TInput, TOutput>
        where TOutput : class
    {
        private readonly ITemplateFactory<TInput, TOutput> _contentTemplateFactory;

        public WrappedHandlerTemplateFactory(ITemplateFactory<TInput, TOutput> contentTemplateFactory)
        {
            EnsureArg.IsNotNull(contentTemplateFactory, nameof(contentTemplateFactory));
            _contentTemplateFactory = contentTemplateFactory;
        }

        public override TOutput Create(TInput input) => _contentTemplateFactory.Create(input);
    }
}
