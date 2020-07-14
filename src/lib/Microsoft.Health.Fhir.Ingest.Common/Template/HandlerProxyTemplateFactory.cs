// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Common.Handler;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public abstract class HandlerProxyTemplateFactory<TInput, TOutput> :
        ITemplateFactory<TInput, TOutput>,
        IResponsibilityHandler<TInput, TOutput>
        where TOutput : class
    {
        private readonly IList<string> _templateErrors = new List<string>();

        protected IList<string> TemplateErrors
        {
            get { return _templateErrors; }
        }

        public abstract TOutput Create(TInput jsonTemplate);

        TOutput IResponsibilityHandler<TInput, TOutput>.Evaluate(TInput request) => Create(request);

        protected JsonSerializer GetJsonSerializer()
        {
            return JsonSerializer.Create(new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = (sender, args) =>
                {
                    _templateErrors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                },
            });
        }
    }
}
