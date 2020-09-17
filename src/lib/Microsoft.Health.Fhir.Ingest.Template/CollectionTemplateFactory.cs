// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using EnsureThat;
using Microsoft.Health.Common.Handler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public abstract class CollectionTemplateFactory<TInTemplate, TOutTemplate> : ITemplateFactory<string, ITemplateContext<TOutTemplate>>
        where TInTemplate : class
        where TOutTemplate : class
    {
        private static readonly IResponsibilityHandler<TemplateContainer, TInTemplate> NotFoundHandler = new TemplateNotFoundHandler<TInTemplate>();

        protected CollectionTemplateFactory(params ITemplateFactory<TemplateContainer, TInTemplate>[] factories)
        {
            EnsureArg.IsNotNull(factories, nameof(factories));
            EnsureArg.HasItems(factories, nameof(factories));

            IResponsibilityHandler<TemplateContainer, TInTemplate> handler = new WrappedHandlerTemplateFactory<TemplateContainer, TInTemplate>(factories[0]);
            for (int i = 1; i < factories.Length; i++)
            {
                handler = handler.Chain(new WrappedHandlerTemplateFactory<TemplateContainer, TInTemplate>(factories[i]));
            }

            // Attach NotFoundHandler at the end of the chain to throw exception if we reach end with no factory found.
            TemplateFactories = handler.Chain(NotFoundHandler);
        }

        protected IResponsibilityHandler<TemplateContainer, TInTemplate> TemplateFactories { get; }

        protected abstract string TargetTemplateTypeName { get; }

        public ITemplateContext<TOutTemplate> Create(string input)
        {
            var templateContext = new TemplateContext<TOutTemplate>();

            TemplateContainer rootContainer = null;
            try
            {
                rootContainer = JsonConvert.DeserializeObject<TemplateContainer>(input);
            }
            catch (JsonSerializationException ex)
            {
                templateContext.Errors.Add(new TemplateError(ex.Message));
            }
            catch (JsonReaderException ex)
            {
                templateContext.Errors.Add(new TemplateError(ex.Message));
            }

            if (rootContainer != null && IsValid(rootContainer, templateContext.Errors))
            {
                templateContext.Template = BuildCollectionTemplate((JArray)rootContainer.Template, templateContext.Errors);
            }

            return templateContext;
        }

        protected abstract TOutTemplate BuildCollectionTemplate(JArray templateCollection, ICollection<TemplateError> errors);

        private bool IsValid(TemplateContainer rootContainer, ICollection<TemplateError> errors)
        {
            if (!rootContainer.MatchTemplateName(TargetTemplateTypeName))
            {
                errors.Add(new TemplateError($"Expected {nameof(rootContainer.TemplateType)} value {TargetTemplateTypeName}, actual {rootContainer.TemplateType ?? "Not Found"}."));
                return false;
            }

            if (rootContainer.Template?.Type != JTokenType.Array)
            {
                errors.Add(new TemplateError($"Expected an array for the template property value for template type {TargetTemplateTypeName}."));
                return false;
            }

            return true;
        }
    }
}
